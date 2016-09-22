using System;
using System.Numerics;
using System.Windows.Forms;
using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
    public interface IPhaseExtractor {
        [NotNull]
        double[] GetPhase([NotNull] double[] symmetryPulse);

        event SpectrumReadyEventHandler RawSpectrumReady;

        event PhaseReadyEventHandler RawPhaseReady;
    }

    public delegate void SpectrumReadyEventHandler(Complex[] spectrum);
    public delegate void PhaseReadyEventHandler(double[] phase);

    public class FourierOnlyPhaseExtractor : IPhaseExtractor {
        private readonly Rotator _rotator = new Rotator();
        private Complex[] _complexContainer;
        private double[] _phaseArray;

        public double[] GetPhase(double[] symmetryPulse) {
            if (_phaseArray == null || _complexContainer == null) {
                _phaseArray = new double[symmetryPulse.Length];
                _complexContainer = new Complex[symmetryPulse.Length];
            }
//            symmetryPulse.ToComplex(_complexContainer);
//            _rotator.Rotate(_complexContainer);

            // rotate & to complex
            Functions.ToComplexRotate(symmetryPulse, _complexContainer);
            Fourier.Forward(_complexContainer, FourierOptions.Matlab);
            RawSpectrumReady?.Invoke(_complexContainer);
            for (var i = 0; i < _complexContainer.Length; i++) {
                _phaseArray[i] = _complexContainer[i].Phase;
            }
            RawPhaseReady?.Invoke(_phaseArray);
            return _phaseArray;
        }

        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;
    }

    public class SpecifiedRangePhaseExtractor : IPhaseExtractor {
        private readonly int _end;
        private readonly double[] _linespace;
        private readonly int _rangeLength;
        private readonly double[] _rangePhaseContainer;
        private readonly Complex[] _rangeSpecContainer;

        private readonly int _start;

        private Complex[] _fullComplexContainer;
        private double[] _fullDoubleContainer;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpecifiedRangePhaseExtractor(int start, int end) {
            _start = start;
            _end = end;
            _rangeLength = end - start + 1;
            _linespace = Functions.LineSpace(start, end, _rangeLength);
            _rangeSpecContainer = new Complex[_rangeLength];
            _rangePhaseContainer = new double[_rangeLength];
        }

        public double[] GetPhase(double[] symmetryPulse) {
            if (_fullDoubleContainer == null || _fullComplexContainer == null) {
                _fullDoubleContainer = new double[symmetryPulse.Length];
                _fullComplexContainer = new Complex[symmetryPulse.Length];
            }
            Functions.ToComplexRotate(symmetryPulse, _fullComplexContainer);
            _fullComplexContainer.FFT();
            // todo including problem
            Array.Copy(_fullComplexContainer, _start, _rangeSpecContainer, 0, _rangeLength);
            RawSpectrumReady?.Invoke(_rangeSpecContainer);
            _rangeSpecContainer.Phase(_rangePhaseContainer);
            Functions.Unwrap(_rangePhaseContainer);
            RawPhaseReady?.Invoke(_rangePhaseContainer);
            var fitFunc = Fit.LineFunc(_linespace, _rangePhaseContainer);
            for (var i = 0; i < symmetryPulse.Length; i++) {
                _fullDoubleContainer[i] = fitFunc(i);
            }
            return _fullDoubleContainer;
        }

        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;
    }

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
        public double[] GetPhase(double[] symmetryPulse) {
            if (_interpolator == null || _phaseArray == null) {
                _interpolator = new Interpolator(_centerHalfWidth*2, symmetryPulse.Length);
                _phaseArray = new double[symmetryPulse.Length];
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

    public class ClassicWrongPhaseExtractor : IPhaseExtractor {
        private readonly CentralInterpolationPhaseExtractor _phaseExtractor;

        public ClassicWrongPhaseExtractor(IApodizer apodizer, int centerHalfWidth) {
            _phaseExtractor = new CentralInterpolationPhaseExtractor(apodizer, centerHalfWidth, Functions.Phase);
        }

        public double[] GetPhase(double[] symmetryPulse) {
            return _phaseExtractor.GetPhase(symmetryPulse);
        }

        public event SpectrumReadyEventHandler RawSpectrumReady {
            add { _phaseExtractor.RawSpectrumReady += value; }
            remove { _phaseExtractor.RawSpectrumReady -= value; }
        }

        public event PhaseReadyEventHandler RawPhaseReady {
            add { _phaseExtractor.RawPhaseReady += value; }
            remove { _phaseExtractor.RawPhaseReady -= value; }
        }
    }

    public class CorrectCenterPhaseExtractor : IPhaseExtractor {
        private readonly CentralInterpolationPhaseExtractor _phaseExtractor;

        public CorrectCenterPhaseExtractor(IApodizer apodizer, int centerHalfWidth) {
            _phaseExtractor = new CentralInterpolationPhaseExtractor(apodizer, centerHalfWidth, complex => complex.Phase);
        }

        public double[] GetPhase(double[] symmetryPulse) {
            return _phaseExtractor.GetPhase(symmetryPulse);
        }

        public event SpectrumReadyEventHandler RawSpectrumReady {
            add { _phaseExtractor.RawSpectrumReady += value; }
            remove { _phaseExtractor.RawSpectrumReady -= value; }
        }

        public event PhaseReadyEventHandler RawPhaseReady {
            add { _phaseExtractor.RawPhaseReady += value; }
            remove { _phaseExtractor.RawPhaseReady -= value; }
        }

    }
}