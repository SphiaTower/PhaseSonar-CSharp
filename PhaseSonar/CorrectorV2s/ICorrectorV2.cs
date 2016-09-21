using System;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
    public interface ICorrectorV2 {
        Complex[] Correct([NotNull] double[] zeroFilledPulse, int crestIndex);
    }


    public interface IPhaseCorrector {
        void Correct([NotNull] Complex[] spectrum, double[] phase, double[] result);
    }

    public class AutoFlipCorrectorV2 : ICorrectorV2 {
        private readonly ICorrectorV2 _delegate;

        public AutoFlipCorrectorV2(ICorrectorV2 delegateCorrector) {
            _delegate = delegateCorrector;
        }

        [NotNull]
        public Complex[] Correct(double[] zeroFilledPulse, int crestIndex) {
            var complexSpectrum = _delegate.Correct(zeroFilledPulse,crestIndex);
            int length = complexSpectrum.Length;
            double sum = 0;
            for (int i = length/4; i < length/2; i++) {
                sum += complexSpectrum[i].Real;
            }
            if (sum<0) {
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

        [CanBeNull] private Complex[] _outputArray;

        [CanBeNull] private Complex[] _spectrumArray;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public MertzCorrectorV2(IPhaseExtractor phaseExtractor, IApodizer apodizer) {
            _apodizer = apodizer;
            _phaseExtractor = phaseExtractor;
        }

        [NotNull]
        public Complex[] Correct(double[] zeroFilledPulse, int crestIndex) {
            if (_spectrumArray == null || _outputArray == null) {
                _spectrumArray = new Complex[zeroFilledPulse.Length];
                _outputArray = new Complex[zeroFilledPulse.Length/2];
            }
//            Toolbox.WriteData(@"D:\zbf\temp\0_zero_filled.txt",zeroFilledPulse);
            // -mean, and zero fill
            // try symmetrize
            if (!_rotator.TrySymmetrize(zeroFilledPulse, crestIndex)) throw new Exception();
//            Toolbox.WriteData(@"D:\zbf\temp\1_symmetrized.txt", zeroFilledPulse);

            // get phase
            var phaseArray = _phaseExtractor.GetPhase(zeroFilledPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\2_phase.txt", phaseArray);

            _apodizer.Apodize(zeroFilledPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\3_apodized.txt", zeroFilledPulse);

            _rotator.Rotate(zeroFilledPulse);
//            Toolbox.WriteData(@"D:\zbf\temp\4_rotated.txt", zeroFilledPulse);

            zeroFilledPulse.ToComplex(_spectrumArray);
            Fourier.Forward(_spectrumArray, FourierOptions.Matlab);
//            Toolbox.WriteData(@"D:\zbf\temp\5_fft.txt", _spectrumArray);

            for (var i = 0; i < _outputArray.Length; i++) {
                var phase = phaseArray[i];
                var real = _spectrumArray[i].Real;
                var imag = _spectrumArray[i].Imaginary;
                _outputArray[i] = real*Math.Cos(phase) + imag*Math.Sin(phase);
            }
//            Toolbox.WriteData(@"D:\zbf\temp\6_output.txt", _outputArray);

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
        public FakeCorrectorV2( IApodizer apodizer) {
            _apodizer = apodizer;
        }

        public Complex[] Correct(double[] zeroFilledPulse, int crestIndex) {
            if (_spectrumArray == null || _outputArray == null) {
                _spectrumArray = new Complex[zeroFilledPulse.Length];
                _outputArray = new Complex[zeroFilledPulse.Length/2];
            }
            // -mean, and zero fill
            // try symmetrize
            if (!_rotator.TrySymmetrize(zeroFilledPulse, crestIndex)) throw new Exception();

            _apodizer.Apodize(zeroFilledPulse);
            _rotator.Rotate(zeroFilledPulse);

            zeroFilledPulse.ToComplex(_spectrumArray);
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