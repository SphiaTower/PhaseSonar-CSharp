using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.PhaseExtractors {
    public interface IPhaseExtractor {
        /// <summary>
        ///     Get the phase spectrum of the input pulse
        /// </summary>
        /// <param name="symmetryPulse">An input pulse with its peak located at the center</param>
        /// <param name="correspondSpectrum">The complex spectrum of the input pulse, could be null</param>
        /// <throws>
        ///     <exception cref="PhaseFitException">thrown when having problems getting the phase</exception>
        /// </throws>
        /// <returns></returns>
        [NotNull]
        double[] GetPhase([NotNull] double[] symmetryPulse, [CanBeNull] Complex[] correspondSpectrum);
    }
}