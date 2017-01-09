using JetBrains.Annotations;

namespace PhaseSonar.Analyzers.WithReference {
    public interface ISplitter {
        /// <summary>
        ///     Process the pulse sequence with reference signals and accumulate results of all pulses of gas and reference respectively
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, containing reference pulses</param>
        /// <returns>The result</returns>
        SplitResult Process([NotNull] double[] pulseSequence);
    }
}