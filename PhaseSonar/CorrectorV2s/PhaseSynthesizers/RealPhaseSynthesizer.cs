using System;
using System.Numerics;

namespace PhaseSonar.CorrectorV2s.PhaseSynthesizers {
    public class RealPhaseSynthesizer : IPhaseSynthesizer {
        public void Synthesize(Complex[] spectrum, double[] phase, Complex[] result) {
            for (var i = 0; i < result.Length; i++) {
                var p = phase[i];
                var real = spectrum[i].Real;
                var imag = spectrum[i].Imaginary;
                result[i] = real*Math.Cos(p) + imag*Math.Sin(p);
            }
        }
    }
}