using System;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.Maths;
using PhaseSonar.Utils;

namespace PhaseSonar.Correctors {
    /// <summary>
    ///     The data container for spectra, which contains not only the spectrum data, also the count of accumulation.
    /// </summary>
    public interface ISpectrum {
        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        int PulseCount { get; set; }

        Complex[] Array { get; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        ISpectrum Clone();

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        bool TryAbsorb([NotNull] ISpectrum another);

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Intensity(int index);

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Real(int index);

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Imag(int index);

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        bool HasImag();

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double AverageIntensity(int index);

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        int Length();

        double Magnitude(int index);
        double Phase(int index);
    }

    public class Spectrum : ISpectrum {
        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public Spectrum(Complex[] data, int pulseCount) {
            Array = data;
            PulseCount = pulseCount;
        }

        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        public int PulseCount { get; set; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        public void Clear() {
            for (var i = 0; i < Array.Length; i++) {
                Array[i] = Complex.Zero;
            }
        }

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public ISpectrum Clone() {
            return new Spectrum(Array, PulseCount);
        }

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another) {
            var spectrum = another as Spectrum;
            Array.Increase(spectrum.Array);
            PulseCount += another.PulseCount;
            return true;
        }

        public double Magnitude(int index) {
            return Array[index].Magnitude;
        }

        public double Phase(int index) {
            return Array[index].Phase;
        }

        public Complex[] Array { get; }

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index) {
            return Math.Pow(Magnitude(index), 2);
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index) {
            return Array[index].Real;
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index) {
            return Array[index].Imaginary;
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag() {
            return !Array[0].Imaginary.Equals(0.0);
        }

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double AverageIntensity(int index) {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length() {
            return Array.Length;
        }
    }

    public class RealSpectrum : ISpectrum {
        private readonly double[] _data;

        public RealSpectrum(double[] _data, int pulseCount) {
            this._data = _data;
            PulseCount = pulseCount;
        }

        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        public int PulseCount { get; set; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        public void Clear() {
            Functions.Clear(_data);
        }

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public ISpectrum Clone() {
            return new RealSpectrum(_data, PulseCount);
        }

        public double Phase(int index) {
            return 0;
        }

        [NotNull]
        public Complex[] Array => _data.ToComplex();

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another) {
            var realSpectrum = another as RealSpectrum;
            if (realSpectrum == null) {
                return false;
            }
            Functions.AddTo(_data, realSpectrum._data);
            PulseCount += another.PulseCount;
            return true;
        }

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index) {
            return _data[index]; // todo
        }

        public double Magnitude(int index) {
            return _data[index];
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index) {
            return _data[index];
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index) {
            return 0;
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag() {
            return false;
        }

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double AverageIntensity(int index) {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length() {
            return _data.Length;
        }
    }
}