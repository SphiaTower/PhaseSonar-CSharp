using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.CorrectorV2s.PhaseSynthesizers {
    public interface IPhaseSynthesizer {
        void Synthesize([NotNull] Complex[] spectrum, [NotNull] double[] phase, [NotNull] Complex[] result);
    }
}