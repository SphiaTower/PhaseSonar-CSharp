using System;
using FTIR.Maths;
using JetBrains.Annotations;

namespace FTIR.Correctors
{
    public interface ISpectrum
    {
        int PulseCount { get; set; }
        void Clear();
        ISpectrum Clone();
        void TryAbsorb(ISpectrum other);
        double Power(int index);
        int Length();
    }


    public class SpectrumFactory<T> where T : ISpectrum
    {
        public static T CreateEmptySpectrum(int size)
        {
            return (T) Activator.CreateInstance(typeof(T), size);
        }

        public static T Clone(T spectrum)
        {
            return (T) spectrum.Clone();
        }
    }


    public class RealSpectrum : ISpectrum
    {
        public RealSpectrum(double[] spectrum, int pulseCount)
        {
            AmplitudeArray = spectrum;
            PulseCount = pulseCount;
        }

        public RealSpectrum(int size)
        {
            AmplitudeArray = new double[size];
            PulseCount = 0;
        }

        public double[] AmplitudeArray { get; }
        public int PulseCount { get; set; }

        public void Clear()
        {
            Funcs.Clear(AmplitudeArray);
            PulseCount = 0;
        }

        public ISpectrum Clone()
        {
            return new RealSpectrum(Funcs.Clone(AmplitudeArray), PulseCount);
        }

        public void TryAbsorb([NotNull] ISpectrum other)
        {
            var realSpectrum = other as RealSpectrum;
            if (realSpectrum == null) throw new InvalidCastException();
            Funcs.AddTo(AmplitudeArray, realSpectrum.AmplitudeArray);
            PulseCount += realSpectrum.PulseCount;
        }

        public double Power(int index)
        {
            return AmplitudeArray[index]*AmplitudeArray[index];
        }

        public int Length()
        {
            return AmplitudeArray.Length;
        }
    }

    public class ComplexSpectrum : ISpectrum
    {
        public ComplexSpectrum(double[] real, double[] imag, int pulseCount)
        {
            RealArray = real;
            ImagArray = imag;
            PulseCount = pulseCount;
        }

        public ComplexSpectrum(int size)
        {
            RealArray = new double[size];
            ImagArray = new double[size];
            PulseCount = 0;
        }

        public double[] RealArray { get; }
        public double[] ImagArray { get; }
        public int PulseCount { get; set; }

        public void Clear()
        {
            Funcs.Clear(RealArray);
            Funcs.Clear(ImagArray);
            PulseCount = 0;
        }


        public ISpectrum Clone()
        {
            return new ComplexSpectrum(Funcs.Clone(RealArray), Funcs.Clone(ImagArray), PulseCount);
        }

        public void TryAbsorb([NotNull] ISpectrum other)
        {
            var complexSpectrum = other as ComplexSpectrum;
            if (complexSpectrum == null) throw new InvalidCastException();
            Funcs.ForceAddTo(RealArray, complexSpectrum.RealArray);
            Funcs.ForceAddTo(ImagArray, complexSpectrum.ImagArray);
            PulseCount += complexSpectrum.PulseCount;
        }

        public double Power(int index)
        {
            return RealArray[index]*RealArray[index] + ImagArray[index]*ImagArray[index];
        }

        public int Length()
        {
            return RealArray.Length;
        }
    }
}