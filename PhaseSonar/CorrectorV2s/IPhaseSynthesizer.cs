using System;
using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.CorrectorV2s {
    public interface IPhaseSynthesizer {
        void Synthesize([NotNull] Complex[] spectrum, [NotNull] double[] phase, [NotNull] Complex[] result);
    }

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

    public class ComplexPhaseSynthesizer : IPhaseSynthesizer {
        public void Synthesize(Complex[] spectrum, double[] phase, Complex[] result) {
            for (var i = 0; i < result.Length; i++) {
                result[i] = spectrum[i]*Complex.Exp(-phase[i]*Complex.ImaginaryOne);
            }
        }
    }
}