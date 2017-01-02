using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.CorrectorV2s {
    public class AutoFlipCorrectorV2 : ICorrectorV2 {
        private readonly ICorrectorV2 _delegate;

        public AutoFlipCorrectorV2(ICorrectorV2 delegateCorrector) {
            _delegate = delegateCorrector;
        }

        [NotNull]
        public Complex[] Correct(double[] symmetryPulse) {
            var complexSpectrum = _delegate.Correct(symmetryPulse);
            var length = complexSpectrum.Length;
            double sum = 0;
            for (var i = length/4; i < length/2; i++) {
                sum += complexSpectrum[i].Real;
            }
            if (sum < 0) {
                for (var i = 0; i < length; i++) {
                    complexSpectrum[i] = -complexSpectrum[i];
                }
            }
            return complexSpectrum;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return _delegate.ToString();
        }
    }
}