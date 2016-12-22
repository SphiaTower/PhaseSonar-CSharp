using System;
using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;
using PhaseSonar.PhaseExtractors;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
    public interface ICorrectorV2 {
        /// <summary>
        /// </summary>
        /// <param name="symmetryPulse"></param>
        /// <throws>
        ///     <exception cref="CorrectFailException"></exception>
        /// </throws>
        /// <returns></returns>
        Complex[] Correct([NotNull] double[] symmetryPulse);
    }


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
//            _phaseExtractor.RawSpectrumReady +=
//                spectrum => Toolbox.WriteData(@"D:\zbf\temp\p_spectrum.txt", spectrum);
//            _phaseExtractor.RawPhaseReady += phase => {
//                Toolbox.WriteData(@"D:\zbf\temp\p_phase.txt", phase);
//                Functions.Unwrap(phase);
//                Toolbox.WriteData(@"D:\zbf\temp\p_phase_unwrap.txt", phase);
//            };
        }

        [NotNull]
        public Complex[] Correct(double[] symmetryPulse) {
            if (_spectrumArray == null || _outputArray == null) {
                _spectrumArray = new Complex[symmetryPulse.Length];
                _outputArray = new Complex[symmetryPulse.Length/2];
            }
//            Toolbox.WriteData(@"D:\zbf\temp\0_zero_filled.txt",zeroFilledPulse);
            // -mean, and zero fill
            // try symmetrize
//            Toolbox.WriteData(@"D:\zbf\temp\1_symmetrized.txt", symmetryPulse);

            _apodizer.Apodize(symmetryPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\3_apodized.txt", symmetryPulse);

            // get phase
//            Toolbox.WriteData(@"D:\zbf\temp\2_phase.txt", phaseArray);

//            _apodizer.Apodize(symmetryPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\3_apodized.txt", symmetryPulse);

            _rotator.Rotate(symmetryPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\4_rotated.txt", symmetryPulse);

            symmetryPulse.ToComplex(_spectrumArray);
            Fourier.Forward(_spectrumArray, FourierOptions.Matlab);
            //            Toolbox.WriteData(@"D:\zbf\temp\5_fft.txt", _spectrumArray);

            double[] phaseArray;
            try {
                phaseArray = _phaseExtractor.GetPhase(symmetryPulse, _spectrumArray);
            } catch (PhaseFitException) {
                throw new CorrectFailException();
            }

            _synthesizer.Synthesize(_spectrumArray, phaseArray, _outputArray);

            // Toolbox.WriteData(@"D:\zbf\temp2\6_output.txt", _outputArray);

            return _outputArray;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return "Mertz Method Correction";
        }
    }

    public class FakeCorrectorV2 : ICorrectorV2 {
        [NotNull] private readonly IApodizer _apodizer;

        [NotNull] private readonly Rotator _rotator = new Rotator();

        [CanBeNull] private Complex[] _outputArray;

        [CanBeNull] private Complex[] _spectrumArray;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public FakeCorrectorV2(IApodizer apodizer) {
            _apodizer = apodizer;
        }

        public Complex[] Correct(double[] symmetryPulse) {
            if (_spectrumArray == null || _outputArray == null) {
                _spectrumArray = new Complex[symmetryPulse.Length];
                _outputArray = new Complex[symmetryPulse.Length/2];
            }
            // -mean, and zero fill
            // try symmetrize

            _apodizer.Apodize(symmetryPulse);
            _rotator.Rotate(symmetryPulse);

            symmetryPulse.ToComplex(_spectrumArray);
            Fourier.Forward(_spectrumArray);

            Array.Copy(_spectrumArray, _outputArray, _outputArray.Length);
            return _outputArray;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return "None Correction";
        }
    }
}