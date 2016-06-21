using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     An analyzer which adds up all results in a pulse sequence.
    ///     This class is targeted for the data with 1 component only.
    /// </summary>
    public abstract class Accumulator : SingleDataRecordProcessor
    {
        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="slicer">A slicer</param>
        protected Accumulator(ISlicer slicer) : base(slicer)
        {
        }

        /// <summary>
        ///     <see cref="IAnalyzerStrategy" />
        /// </summary>
        protected abstract IAnalyzerStrategy Strategy { get; set; }

        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum of the pulse sequence, or null if failed</returns>
        [CanBeNull]
        public ISpectrum Accumulate(double[] pulseSequence)
        {
            IList<IList<int>> startIndicesList;
            if (Slicer.Slice(pulseSequence, out startIndicesList))
            {
                var spectra = Strategy.Process(pulseSequence, startIndicesList, Slicer.SlicedPeriodLength, Slicer.CrestIndex);
                return spectra?[0];
            }
            return null;


        }
    }
}