using JetBrains.Annotations;

namespace PhaseSonar.Analyzers.PhaseAnalyzers {
    public interface IPhaseReader {
        /// <summary>
        /// Get the phase spectrum of the input pulse sequence
        /// </summary>
        /// <param name="pulseSequence">A temporal pulse sequence</param>
        /// <returns>The result of the calculation</returns>
        PhaseResult GetPhase([NotNull] double[] pulseSequence);
    }
}