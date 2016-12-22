using System;
using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;
using PhaseSonar.Utils;

namespace PhaseSonar.PhaseExtractors {
    public class CentralInterpolationPhaseExtractor : IPhaseExtractor {
        [NotNull] private readonly IApodizer _apodizer;

        [NotNull] private readonly Complex[] _centerComplexContainer;

        private readonly int _centerHalfWidth;

        [NotNull] private readonly double[] _centerRealContainer;
        private readonly Func<Complex, double> _complexToPhaseFunc;

        [NotNull] private readonly Rotator _rotator = new Rotator();

        [CanBeNull] private Interpolator _interpolator;

        [CanBeNull] private double[] _phaseArray;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public CentralInterpolationPhaseExtractor(IApodizer apodizer, int centerHalfWidth,
            Func<Complex, double> complexToPhaseFunc) {
            _apodizer = apodizer;
            _centerHalfWidth = centerHalfWidth;
            _complexToPhaseFunc = complexToPhaseFunc;
            var centerLength = centerHalfWidth*2;
            _centerRealContainer = new double[centerLength];
            _centerComplexContainer = new Complex[centerLength];
        }

        [NotNull]
        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            if (_interpolator == null || _phaseArray == null) {
                _interpolator = new Interpolator(_centerHalfWidth*2, symmetryPulse.Length);
                _phaseArray = new double[symmetryPulse.Length/2];
            }
            // extract central portion
            var centerBurst = symmetryPulse.Length/2;
            var centralPulse = _centerRealContainer;
            Array.Copy(symmetryPulse, centerBurst - _centerHalfWidth, centralPulse, 0, _centerHalfWidth*2);
            // apodize & rotate
            _apodizer.Apodize(centralPulse);
            _rotator.Rotate(centralPulse);
            // fft
            _centerRealContainer.ToComplex(_centerComplexContainer);
            Fourier.Forward(_centerComplexContainer, FourierOptions.Matlab);
//            Toolbox.WriteData(@"D:\zbf\temp\central_fft.txt", _centerComplexContainer);
            RawSpectrumReady?.Invoke(_centerComplexContainer);
            // get phase from spectrum
            var complexSpectrum = _centerComplexContainer;
            for (var i = 0; i < _centerRealContainer.Length; i++) {
                _centerRealContainer[i] = _complexToPhaseFunc(complexSpectrum[i]);
            }
            RawPhaseReady?.Invoke(_centerRealContainer);
//            Toolbox.WriteData(@"D:\zbf\temp\central_phase.txt", _centerRealContainer);
            // interpolate into full length
            _interpolator.Interpolate(_centerRealContainer, _phaseArray);
            return _phaseArray;
        }

        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;
    }
}