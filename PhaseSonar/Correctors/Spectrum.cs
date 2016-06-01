using System;
using PhaseSonar.Maths;
using JetBrains.Annotations;

namespace PhaseSonar.Correctors
{
    /// <summary>
    /// The data container for spectra.
    /// </summary>
    public interface ISpectrum
    {
        /// <summary>
        /// The number of pulses that are accumulated in this container.
        /// </summary>
        int PulseCount { get; set; }
        /// <summary>
        /// Clear the container.
        /// </summary>
        void Clear();
        /// <summary>
        /// Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        ISpectrum Clone();
        /// <summary>
        /// Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        void TryAbsorb([NotNull]ISpectrum another);
        /// <summary>
        /// Get the intensity of the data at a specified index
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Intensity(int index);
        /// <summary>
        /// Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        int Length();
    }

    /// <summary>
    /// A generic factory which creates spectrum containers of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the spectrum</typeparam>
    public class SpectrumFactory<T> where T : ISpectrum
    {
        /// <summary>
        /// Create empty spectrum containers of the specified type.
        /// </summary>
        /// <param name="size">The size of the container</param>
        /// <returns>An empty ISpectrum instance</returns>
        public static T CreateEmptySpectrum(int size)
        {
            return (T) Activator.CreateInstance(typeof(T), size);
        }
    }

    /// <summary>
    /// A spectrum that contains only real numbers
    /// </summary>
    public class RealSpectrum : ISpectrum
    {
        /// <summary>
        /// Create a real spectrum instance
        /// </summary>
        /// <param name="spectrum">The data of the spectrum</param>
        /// <param name="pulseCount">The number of pulses accumulated</param>
        public RealSpectrum(double[] spectrum, int pulseCount)
        {
            AmplitudeArray = spectrum;
            PulseCount = pulseCount;
        }

        /// <summary>
        /// Create an empty container
        /// </summary>
        /// <param name="size">The size of the container</param>
        public RealSpectrum(int size)
        {
            AmplitudeArray = new double[size];
            PulseCount = 0;
        }

        /// <summary>
        /// The amplitude data
        /// </summary>
        public double[] AmplitudeArray { get; }
        /// <summary>
        /// The number of pulses accumulated
        /// </summary>
        public int PulseCount { get; set; }
        /// <summary>
        /// Clear the container.
        /// </summary>
        public void Clear()
        {
            Functions.Clear(AmplitudeArray);
            PulseCount = 0;
        }

        /// <summary>
        /// Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        public ISpectrum Clone() {
            return new RealSpectrum(Functions.Clone(AmplitudeArray), PulseCount);
        }

        /// <summary>
        /// Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public void TryAbsorb( ISpectrum another) {
            var realSpectrum = another as RealSpectrum;
            if (realSpectrum == null) throw new InvalidCastException();
            Functions.AddTo(AmplitudeArray, realSpectrum.AmplitudeArray);
            PulseCount += realSpectrum.PulseCount;
        }

        /// <summary>
        /// Get the intensity of the data at a specified index
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index) {
            return AmplitudeArray[index] * AmplitudeArray[index];
        }

        /// <summary>
        /// Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length() {
            return AmplitudeArray.Length;
        }
    }
    /// <summary>
    /// A spectrum that contains only complex numbers
    /// </summary>
    public class ComplexSpectrum : ISpectrum
    {
        /// <summary>
        /// Create a complex spectrum instance
        /// </summary>
        /// <param name="real">The real part of the spectrum</param>
        /// <param name="imag">The imag part of the spectrum</param>
        /// <param name="pulseCount">The number of pulses accumulated</param>
        public ComplexSpectrum(double[] real, double[] imag, int pulseCount)
        {
            RealArray = real;
            ImagArray = imag;
            PulseCount = pulseCount;
        }
        /// <summary>
        /// Create an empty container
        /// </summary>
        /// <param name="size">The size of the container</param>
        public ComplexSpectrum(int size)
        {
            RealArray = new double[size];
            ImagArray = new double[size];
            PulseCount = 0;
        }
        /// <summary>
        /// The real part.
        /// </summary>
        public double[] RealArray { get; }
        /// <summary>
        /// The imag part
        /// </summary>
        public double[] ImagArray { get; }

        /// <summary>
        /// The number of pulses that are accumulated in this container.
        /// </summary>
        public int PulseCount { get; set; }

        /// <summary>
        /// Clear the container.
        /// </summary>
        public void Clear() {
            Functions.Clear(RealArray);
            Functions.Clear(ImagArray);
            PulseCount = 0;
        }


        /// <summary>
        /// Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        public ISpectrum Clone() {
            return new ComplexSpectrum(Functions.Clone(RealArray), Functions.Clone(ImagArray), PulseCount);
        }

        /// <summary>
        /// Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public void TryAbsorb(ISpectrum another) {
            var complexSpectrum = another as ComplexSpectrum;
            if (complexSpectrum == null) throw new InvalidCastException();
            Functions.ForceAddTo(RealArray, complexSpectrum.RealArray);
            Functions.ForceAddTo(ImagArray, complexSpectrum.ImagArray);
            PulseCount += complexSpectrum.PulseCount;
        }

        /// <summary>
        /// Get the intensity of the data at a specified index
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index) {
            return RealArray[index] * RealArray[index] + ImagArray[index] * ImagArray[index];
        }


        /// <summary>
        /// Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length() {
            return RealArray.Length;
        }

    }
}