using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics;
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
            if (spectrum == null) {
                return false;
            }
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
            return Array[index].MagnitudeSquared();
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
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length() {
            return Array.Length;
        }
    }
}