using System;
using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;
using PhaseSonar.Maths.Apodizers;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
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