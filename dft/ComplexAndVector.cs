using System;
using System.Collections.Generic;
using System.Linq;

namespace dft
{
    class Complex
    {
        double real_;
        double imag_;

        public Complex() : this(0) { }

        public Complex(double real) : this(real, 0) { }

        public Complex(double real, double imag)
        {
            real_ = real;
            imag_ = imag;
        }

        public static Complex operator +(Complex lhs, Complex rhs)
        {
            return new Complex(lhs.real_ + rhs.real_, lhs.imag_ + rhs.imag_);
        }

        public static Complex operator -(Complex lhs)
        {
            return new Complex(-lhs.real_, -lhs.imag_);
        }

        public static Complex operator -(Complex lhs, Complex rhs)
        {
            return lhs + (-rhs);
        }

        public static Complex operator *(Complex lhs, Complex rhs)
        {
            Complex r = new Complex();
            r.real_ = lhs.real_ * rhs.real_ - lhs.imag_ * rhs.imag_;
            r.imag_ = lhs.real_ * rhs.imag_ + lhs.imag_ * rhs.real_;
            return r;
        }

        public static Complex operator /(Complex lhs, Complex rhs)
        {
            Complex r = lhs * rhs.Conjugate;
            Complex den = rhs * rhs.Conjugate;
            if (den.Real == 0)
                throw new DivideByZeroException();
            r.real_ /= den.Real;
            r.imag_ /= den.Real;
            return r;
        }

        public Complex Conjugate
        {
            get
            {
                return new Complex(real_, -imag_);
            }
        }

        public void SetEulerFormula(double x)
        {
            real_ = Math.Cos(x);
            imag_ = Math.Sin(x);
        }

        public double Real { get { return real_; } }
        public double Imag { get { return imag_; } }

        public override string ToString()
        {
            if (imag_ < 0)
                return string.Format("{0:0.000}{1:+0.000}i", real_, imag_);
            else
                return string.Format("{0:0.000}{1:+0.000}i", real_, imag_);
        }
    }

    class ComplexVector
    {
        Complex[] data_;

        ComplexVector()
        {
        }

        public ComplexVector(int len)
        {
            data_ = Enumerable.Range(0, len).Select(r => new Complex()).ToArray();
        }

        public int Len { get { return data_.Length; } }

        public Complex this[int index]
        {
            get { return data_[index]; }
            set { data_[index] = value; }
        }

        public static ComplexVector operator +(ComplexVector lhs, ComplexVector rhs)
        {
            if (lhs.Len != rhs.Len)
                throw new InvalidOperationException();

            ComplexVector r = new ComplexVector(lhs.Len);
            for (int i = 0; i < lhs.Len; ++i)
                r[i] = lhs[i] + rhs[i];
            return r;
        }

        public static ComplexVector operator -(ComplexVector lhs)
        {
            ComplexVector r = new ComplexVector();
            r.data_ = lhs.data_.Select(x => -x).ToArray();
            return r;
        }

        public static ComplexVector operator -(ComplexVector lhs, ComplexVector rhs)
        {
            return lhs + (-rhs);
        }

        public static ComplexVector operator *(ComplexVector lhs, Complex rhs)
        {
            ComplexVector r = new ComplexVector();
            r.data_ = lhs.data_.Select(x => x * rhs.Conjugate).ToArray();
            return r;
        }

        public static Complex operator *(ComplexVector lhs, ComplexVector rhs)
        {
            if (lhs.Len != rhs.Len)
                throw new InvalidOperationException();

            Complex r = new Complex();
            for (int i = 0; i < lhs.Len; ++i)
            {
                r = r + lhs[i] * rhs[i].Conjugate;
            }
            return r;
        }

        public void SetFourierSequence(int cycle)
        {
            for (int i = 0; i < Len; ++i)
                data_[i].SetEulerFormula(-2 * Math.PI * cycle * (double)i / Len);
        }
    }
}
