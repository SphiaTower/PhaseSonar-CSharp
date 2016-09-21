using System;
using System.Numerics;
using JetBrains.Annotations;
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

        void Absorb([NotNull] Complex[] spectrum);

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
    }

    public class Spectrum : ISpectrum {
        private readonly Complex[] _data;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public Spectrum(Complex[] data, int pulseCount) {
            _data = data;
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
            for (var i = 0; i < _data.Length; i++) {
                _data[i] = Complex.Zero;
            }
        }

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public ISpectrum Clone() {
            return new Spectrum(_data, PulseCount);
        }

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another) {
            var spectrum = another as Spectrum;
            _data.Increase(spectrum._data);
            PulseCount += another.PulseCount;
            return true;
        }

        public void Absorb(Complex[] spectrum) {
            _data.Increase(spectrum);
            PulseCount ++;
        }

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index) {
            var magnitude = _data[index].Magnitude;
            return Math.Pow(magnitude, 2);
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index) {
            return _data[index].Real;
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index) {
            return _data[index].Imaginary;
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag() {
            return !_data[0].Imaginary.Equals(0.0);
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