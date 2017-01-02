using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.CorrectorV2s.PhaseSynthesizers;
using PhaseSonar.Maths;
using PhaseSonar.Maths.Apodizers;
using PhaseSonar.PhaseExtractors;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
    public class MertzCorrectorV2 : ICorrectorV2 {
        [NotNull] private readonly IApodizer _apodizer;

        [NotNull] private readonly IPhaseExtractor _phaseExtractor;

        [NotNull] private readonly Rotator _rotator = new Rotator();
        private readonly IPhaseSynthesizer _synthesizer;

        [CanBeNull] private Complex[] _outputArray;

        [CanBeNull] private Complex[] _spectrumArray;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public MertzCorrectorV2(IPhaseExtractor phaseExtractor, IApodizer apodizer, IPhaseSynthesizer synthesizer) {
            _apodizer = apodizer;
            _synthesizer = synthesizer;
            _phaseExtractor = phaseExtractor;
        }

        [NotNull]
        public Complex[] Correct(double[] symmetryPulse) {
            if (_spectrumArray == null || _outputArray == null) {
                _spectrumArray = new Complex[symmetryPulse.Length];
                _outputArray = new Complex[symmetryPulse.Length/2];
            }

            _apodizer.Apodize(symmetryPulse);

            _rotator.Rotate(symmetryPulse);

            symmetryPulse.ToComplex(_spectrumArray);
            Fourier.Forward(_spectrumArray, FourierOptions.Matlab);

            double[] phaseArray;
            try {
                phaseArray = _phaseExtractor.GetPhase(symmetryPulse, _spectrumArray);
            } catch (PhaseFitException) {
                throw new CorrectFailException();
            }

            _synthesizer.Synthesize(_spectrumArray, phaseArray, _outputArray);


            return _outputArray;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return "Mertz Method Correction";
        }
    }
}