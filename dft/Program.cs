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

        static bool DblEq(double lhs, double rhs)
        {
            double diff = Math.Abs(lhs * 0.00001);
            if (Math.Abs(lhs - rhs) <= diff)
                return true;
            else
                return false;
        }

        static void Main(string[] args)
        {
            short[] music = ReadPCM(@"d:\nr\music.pcm").Take(44100 * 2 * 60).ToArray();

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

            double prevHakuDur = 0;
            double prevOffset = 0;
            for (int i = 0; i < 4; ++i)
            {
                Complex c = cpx[maxIndex];
                
                double modulus = c.GetModulus();
                double cosoffset = Math.Acos(c.Re / modulus);
                double sinoffset = Math.Asin(c.Im / modulus);

                if (c.Re > 0 && c.Im > 0)
                {
                    ;
                }
                else if (c.Re > 0 && c.Im < 0)
                {
                    cosoffset = 2 * Math.PI - cosoffset;
                    sinoffset = 2 * Math.PI + sinoffset;
                }
                else if (c.Re < 0 && c.Im < 0)
                {
                    cosoffset = 2 * Math.PI - cosoffset;
                    sinoffset = Math.PI - sinoffset;
                }
                else if (c.Re < 0 && c.Im > 0)
                {
                    sinoffset = Math.PI - sinoffset;
                }

                double offsetTime = 60.0 * (cosoffset / 2 / Math.PI / maxIndex);

                Console.WriteLine("bpm: {0}", maxIndex);
                Console.WriteLine("factor: {0}", c.ToString());
                Console.WriteLine("angle: {0}", cosoffset);
                Console.WriteLine("offset: {0}", offsetTime);

                if (!DblEq(prevHakuDur, 0))
                {
                    Console.WriteLine("haku offset: {0}", (offsetTime - prevOffset) / prevHakuDur);
                }

                Console.WriteLine();

                prevHakuDur = 60.0 / maxIndex;
                prevOffset = offsetTime;

                maxIndex = maxIndex / 2;
            }

            File.WriteAllText(@"d:\nr\fft.csv",
                new string(cpx.SelectMany(x => (x.GetModulus().ToString() + "\t" + x.ToString() + "\r\n").ToCharArray()).ToArray())
            );
        }
    }
}
