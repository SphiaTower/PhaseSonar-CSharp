using System.Numerics;
using PhaseSonar.Maths.Apodizers;

namespace PhaseSonar.PhaseExtractors {
    public class CorrectCenterPhaseExtractor : IPhaseExtractor {
        private readonly CentralInterpolationPhaseExtractor _phaseExtractor;

        public CorrectCenterPhaseExtractor(IApodizer apodizer, int centerHalfWidth) {
            _phaseExtractor = new CentralInterpolationPhaseExtractor(apodizer, centerHalfWidth, complex => complex.Phase);
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            return _phaseExtractor.GetPhase(symmetryPulse, correspondSpectrum);
        }


    }
}