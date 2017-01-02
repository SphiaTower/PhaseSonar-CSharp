namespace PhaseSonar.Maths.Apodizers {
    /// <summary>
    ///     An apodizer which applies a triangluar window.
    /// </summary>
    public class TriangulerApodizer : IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse) {
            var centerBurst = symmetryPulse.Length/2;
            var rampArray = RampGenerator.Ramp(symmetryPulse.Length, centerBurst);
            Functions.MultiplyInPlace(symmetryPulse, rampArray);
        }
    }
}