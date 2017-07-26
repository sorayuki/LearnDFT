using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dft
{
    class Program
    {
        static void Main(string[] args)
        {
            ComplexVector vec = new ComplexVector(16);
            for(int i = 0; i < 16; ++i)
            {
                ComplexVector tmp = new ComplexVector(16);
                tmp.SetFourierSequence(i);
                tmp = tmp * (new Complex(i));
                vec = vec + tmp;
            }

            for (int i = 0; i < 16; ++i)
            {
                ComplexVector tmp = new ComplexVector(16);
                tmp.SetFourierSequence(i);

                Complex x = vec * tmp;
                Complex k = tmp * tmp;
                Console.WriteLine("{0}", x / k);
            }
        }
    }
}
