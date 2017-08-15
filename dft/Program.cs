using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using Exocortex.DSP;

namespace dft
{
    class Program
    {
        static short[] ReadPCM(string file)
        {
            var tmp = File.ReadAllBytes(file);
            short[] r = new short[tmp.Length / 2];
            for(int i = 0; i < r.Length; ++i)
            {
                r[i] = (short)((tmp[i * 2] | (tmp[i * 2 + 1] << 8)));
            }

            return r;
        }

        static Tuple<Complex[], Complex[]> ToComplexVector(short[] data, int offset, int len)
        {
            Complex[] l = new Complex[len];
            Complex[] r = new Complex[len];

            for (int i = 0; i < len; ++i)
            {
                l[i].Re = data[offset + i * 2];
                l[i].Im = 0;
                r[i].Re = data[offset + i * 2 + 1];
                r[i].Im = 0;
            }

            return Tuple.Create(l, r);
        }

        static void Main(string[] args)
        {
            double[] spectruml = new double[2048];
            double[] spectrumr = new double[2048];
            double[] gain = new double[2048];
            double gainMin = 1e-6;
            double gainMax = 1.0;

            for (int i = 0; i < 2048; ++i)
                gain[i] = 1.0;

            short[] noiseData = ReadPCM(@"d:\nr\noise.pcm");
            int noiseTotal = 44100 * 2 * 2/*noiseData.Length*/;
            Console.WriteLine("analyzing");
            int lastProgress = 0;
            double den = 0;
            for (int i = 0; i < noiseTotal; i += 2 * 2048)
            {
                int curProgress = i * 100 / noiseTotal;
                if (curProgress > lastProgress)
                {
                    Console.WriteLine("{0}%", curProgress);
                    lastProgress = curProgress;
                }
                var signals = ToComplexVector(noiseData, i, 2048);
                Fourier.FFT(signals.Item1, 2048, FourierDirection.Forward);
                Fourier.FFT(signals.Item2, 2048, FourierDirection.Forward);

                for (int j = 0; j < 2048; ++j)
                {
                    double lmod = signals.Item1[j].GetModulus();
                    double rmod = signals.Item2[j].GetModulus();
                    spectruml[j] += lmod;
                    spectrumr[j] += rmod;
                }
                den += 1;
            }

            for (int i = 0; i < 2048; ++i)
            {
                spectruml[i] /= den;
                spectrumr[i] /= den;
            }

            noiseData = null;

            short[] musicData = ReadPCM(@"d:\nr\todo.pcm");
            MemoryStream ms = new MemoryStream();
            int musicTotal = musicData.Length - 2 * 2048;
            lastProgress = 0;
            for (int i = 0; i < musicTotal; i += 1 * 2048)
            {
                int curProgress = i * 100 / musicTotal;
                if (curProgress > lastProgress)
                {
                    Console.WriteLine("{0}%", curProgress);
                    lastProgress = curProgress;
                }

                var signals = ToComplexVector(musicData, i, 2048);
                Fourier.FFT(signals.Item1, 2048, FourierDirection.Forward);
                Fourier.FFT(signals.Item2, 2048, FourierDirection.Forward);

                var ldft = signals.Item1;
                var rdft = signals.Item2;

                for(int j = 0; j < 2048; ++j)
                {
                    if (ldft[j].GetModulus() < spectruml[j] * 1.2 || rdft[j].GetModulus() < spectrumr[j] * 1.2)
                    {
                        gain[j] *= 0.1;
                    }
                    else
                    {
                        gain[j] *= 20;
                    }

                    if (gain[j] > gainMax) gain[j] = gainMax;
                    else if (gain[j] < gainMin) gain[j] = gainMin;

                    ldft[j] *= gain[j];
                    rdft[j] *= gain[j];

                    ldft[j] /= 2048;
                    rdft[j] /= 2048;
                }

                ldft[0].Re = 0;
                ldft[0].Im = 0;
                rdft[0].Re = 0;
                rdft[0].Im = 0;

                Fourier.FFT(ldft, 2048, FourierDirection.Backward);
                Fourier.FFT(rdft, 2048, FourierDirection.Backward);

                for(int j = 512; j < 1536; ++j)
                {
                    short l, r;
                    if (ldft[j].Re > 32767)
                        l = 32767;
                    else if (ldft[j].Re < -32768)
                        l = -32768;
                    else
                        l = (short)rdft[j].Re;

                    if (rdft[j].Re > 32767)
                        r = 32767;
                    else if (rdft[j].Re < -32768)
                        r = -32768;
                    else
                        r = (short)rdft[j].Re;


                    ms.WriteByte((byte)(l & 0xff));
                    ms.WriteByte((byte)((l >> 8) & 0xff));
                    ms.WriteByte((byte)(r & 0xff));
                    ms.WriteByte((byte)((r >> 8) & 0xff));
                }
            }

            File.WriteAllBytes(@"d:\nr\done.pcm", ms.ToArray());
        }
    }
}
