using JetBrains.Annotations;

namespace PhaseSonar.Analyzers.WithoutReference {
    /// <summary>
    ///     An basic analyser which slices and processes the pulse sequence.
    /// </summary>
    public interface IAccumulator {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, without reference signals</param>
        /// <returns>The accumulated spectrum</returns>
        AccumulationResult Process([NotNull] double[] pulseSequence);
    }
}