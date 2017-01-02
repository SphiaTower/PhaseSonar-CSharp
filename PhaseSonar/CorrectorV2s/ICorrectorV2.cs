using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.PhaseExtractors;

namespace PhaseSonar.CorrectorV2s {
    public interface ICorrectorV2 {
        /// <summary>
        /// </summary>
        /// <param name="symmetryPulse"></param>
        /// <throws>
        ///     <exception cref="CorrectFailException"></exception>
        /// </throws>
        /// <returns></returns>
        Complex[] Correct([NotNull] double[] symmetryPulse);
    }
}