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
            short[] music = ReadPCM(@"d:\nr\music.pcm").Take(44100 * 2 * 30).ToArray();

            int pieceCount = 4096;

            double[] avgPower = new double[pieceCount];

            for (int i = 0; i < pieceCount; ++i)
            {
                double sum = 0;
                int s = (int)((long)music.Length / 2 * i / pieceCount);
                int c = (int)((long)music.Length / 2 / pieceCount);
                for(int j = 0; j < c; ++j)
                {
                    double sample = music[(s + j) * 2];
                    sum += sample * sample / 32767.0 / 32767.0;
                }
                avgPower[i] = sum / c;
            }

            Complex[] cpx = avgPower.Select(x => Complex.FromRealImaginary(x, 0)).ToArray();
            Fourier.FFT(cpx, cpx.Length, FourierDirection.Forward);

            int maxIndex = cpx.Length / 20;
            for (int i = maxIndex; i < cpx.Length / 2; ++i)
            {
                if (cpx[i].GetModulus() > cpx[maxIndex].GetModulus())
                    maxIndex = i;
            }

            double modulusSqr = cpx[maxIndex].GetModulusSquared();
            double cosoffset = Math.Acos(cpx[maxIndex].Re * cpx[maxIndex].Re / modulusSqr);
            double sinoffset = Math.Asin(cpx[maxIndex].Im * cpx[maxIndex].Im / modulusSqr);

            Console.WriteLine("bpm: {0}", maxIndex);
            Console.WriteLine("offset: {0}", 60.0 / maxIndex * cosoffset / 2 / Math.PI);

            File.WriteAllText(@"d:\nr\fft.csv",
                new string(cpx.SelectMany(x => (x.GetModulus().ToString() + "\t" + x.ToString() + "\r\n").ToCharArray()).ToArray())
            );
        }
    }
}
