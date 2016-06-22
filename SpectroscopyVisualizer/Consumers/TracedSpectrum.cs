using PhaseSonar.Correctors;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     A decorator class for <see cref="ISpectrum" />. It provides a <see cref="Tag" /> property to mark and trace the
    ///     spectrum.
    /// </summary>
    public class TracedSpectrum : ISpectrum
    {
        private readonly ISpectrum _spectrum;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TracedSpectrum(ISpectrum spectrum, string tag)
        {
            _spectrum = spectrum;
            Tag = tag;
        }

        /// <summary>
        ///     A Tag used to mark the instance.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        public int PulseCount
        {
            get { return _spectrum.PulseCount; }
            set { _spectrum.PulseCount = value; }
        }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        public void Clear()
        {
            _spectrum.Clear();
        }

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        public ISpectrum Clone()
        {
            return _spectrum.Clone();
        }

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another)
        {
            return _spectrum.TryAbsorb(another);
        }

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index)
        {
            return _spectrum.Intensity(index);
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index)
        {
            return _spectrum.Real(index);
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index)
        {
            return _spectrum.Imag(index);
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag()
        {
            return _spectrum.HasImag();
        }

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double AverageIntensity(int index)
        {
            return _spectrum.AverageIntensity(index);
        }

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length()
        {
            return _spectrum.Length();
        }
    }
}