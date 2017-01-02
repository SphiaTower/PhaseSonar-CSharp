using JetBrains.Annotations;

namespace PhaseSonar.Analyzers.PhaseAnalyzers {
    public interface IPhaseReader {
        PhaseResult GetPhase([NotNull] double[] pulseSequence);
    }
}