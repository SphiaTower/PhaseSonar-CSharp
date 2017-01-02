using System.Numerics;

namespace PhaseSonar.CorrectorV2s.PhaseSynthesizers {
    public class ComplexPhaseSynthesizer : IPhaseSynthesizer {
        public void Synthesize(Complex[] spectrum, double[] phase, Complex[] result) {
            for (var i = 0; i < result.Length; i++) {
                result[i] = spectrum[i]*Complex.Exp(-phase[i]*Complex.ImaginaryOne);
            }
        }
    }
}