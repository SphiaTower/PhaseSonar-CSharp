using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     An basic analyser which slices and processes the pulse sequence.
    /// </summary>
    public interface IPulseSequenceProcessor {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        Maybe<ISpectrum> Process([NotNull] double[] pulseSequence);
    }
}