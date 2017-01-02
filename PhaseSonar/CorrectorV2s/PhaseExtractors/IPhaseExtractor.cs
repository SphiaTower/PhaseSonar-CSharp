using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.PhaseExtractors {
    public interface IPhaseExtractor {
        /// <summary>
        /// </summary>
        /// <param name="symmetryPulse"></param>
        /// <param name="correspondSpectrum"></param>
        /// <throws>
        ///     <exception cref="PhaseFitException"></exception>
        /// </throws>
        /// <returns></returns>
        [NotNull]
        double[] GetPhase([NotNull] double[] symmetryPulse, [CanBeNull] Complex[] correspondSpectrum);

        event SpectrumReadyEventHandler RawSpectrumReady;

        event PhaseReadyEventHandler RawPhaseReady;
    }
}