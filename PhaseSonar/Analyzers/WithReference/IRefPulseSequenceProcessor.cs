using JetBrains.Annotations;

namespace PhaseSonar.Analyzers.WithReference {
    public interface IRefPulseSequenceProcessor {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        SplitResult Process([NotNull] double[] pulseSequence);
    }
}