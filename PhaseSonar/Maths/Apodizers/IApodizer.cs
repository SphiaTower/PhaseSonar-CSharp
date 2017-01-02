using JetBrains.Annotations;

namespace PhaseSonar.Maths.Apodizers {
    /// <summary>
    ///     An apodizer which applies temporal windows on the signal.
    /// </summary>
    public interface IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        void Apodize([NotNull] double[] symmetryPulse);
    }
}