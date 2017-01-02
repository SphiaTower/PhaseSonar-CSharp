using System;
using System.Numerics;
using PhaseSonar.Maths.Apodizers;

namespace PhaseSonar.PhaseExtractors {
    public class ClassicWrongPhaseExtractor : IPhaseExtractor {
        private readonly CentralInterpolationPhaseExtractor _phaseExtractor;

        public ClassicWrongPhaseExtractor(IApodizer apodizer, int centerHalfWidth) {
            _phaseExtractor = new CentralInterpolationPhaseExtractor(apodizer, centerHalfWidth, Phase);
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            return _phaseExtractor.GetPhase(symmetryPulse, correspondSpectrum);
        }

        public event SpectrumReadyEventHandler RawSpectrumReady {
            add { _phaseExtractor.RawSpectrumReady += value; }
            remove { _phaseExtractor.RawSpectrumReady -= value; }
        }

        public event PhaseReadyEventHandler RawPhaseReady {
            add { _phaseExtractor.RawPhaseReady += value; }
            remove { _phaseExtractor.RawPhaseReady -= value; }
        }

        private static double Phase(double real, double imag) {
            if (Math.Abs(real) > 0.0000001) {
                return Math.Atan(imag/real);
            }
            if (imag > 0) {
                return Math.PI/2;
            }
            if (imag < 0) {
                return -Math.PI/2;
            }
            return 0;
        }

        private static double Phase(Complex complex) {
            return Phase(complex.Real, complex.Imaginary);
            //            return Math.Atan2(complex.Imaginary,complex.Real);
        }
    }
}