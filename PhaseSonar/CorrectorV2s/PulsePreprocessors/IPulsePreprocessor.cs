using JetBrains.Annotations;

namespace PhaseSonar.CorrectorV2s.PulsePreprocessors {
    public interface IPulsePreprocessor {
        [NotNull]
        double[] RetrievePulse([NotNull] double[] pulseSequence, int startIndex, int crestIndexOffStart, int pulseLength);
    }
}