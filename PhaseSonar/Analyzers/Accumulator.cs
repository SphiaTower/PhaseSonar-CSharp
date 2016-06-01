using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     An analyzer which adds up all results in a pulse sequence.
    ///     This class is targeted for the data with 1 component only.
    /// </summary>
    /// <typeparam name="T">The type of spectrum</typeparam>
    public abstract class Accumulator<T> : BaseAnalyzer where T : ISpectrum
    {
        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="slicer">A slicer</param>
        protected Accumulator(ISlicer slicer) : base(slicer)
        {
        }

        /// <summary>
        ///     <see cref="IAnalyzerStrategy{T}" />
        /// </summary>
        protected abstract IAnalyzerStrategy<T> Strategy { get; set; }

        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum of the pulse sequence, or null if failed</returns>
        [CanBeNull]
        public T Accumulate(double[] pulseSequence)
        {
            var startIndicesList = Slicer.Slice(pulseSequence);
            if (startIndicesList == null)
            {
                return default(T);
            }

            var spectra = Strategy.Process(pulseSequence, startIndicesList, Slicer.SlicedPeriodLength, Slicer.CrestIndex);
            return spectra != null ? spectra[0] : default(T);
        }
    }
}