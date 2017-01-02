namespace PhaseSonar.Maths.Apodizers {
    /// <summary>
    ///     An fake apodizer which does nothing in fact.
    /// </summary>
    public class FakeApodizer : IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse) {
        }
    }
}