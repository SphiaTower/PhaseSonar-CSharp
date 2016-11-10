using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using PhaseSonar.Maths;
using PhaseSonar.Utils;

namespace PhaseSonar.CorrectorV2s {
    public interface IPhaseExtractor {
        /// <summary>
        /// </summary>
        /// <param name="symmetryPulse"></param>
        /// <param name="correspondSpectrum"></param>
        /// <throws>
        ///     <exception cref="PhaseFitException"></exception>
        /// </throws>
        /// <returns></returns>
        [NotNull]
        double[] GetPhase([NotNull] double[] symmetryPulse, [CanBeNull] Complex[] correspondSpectrum);

        event SpectrumReadyEventHandler RawSpectrumReady;

        event PhaseReadyEventHandler RawPhaseReady;
    }

    public delegate void SpectrumReadyEventHandler(Complex[] spectrum);

    public delegate void PhaseReadyEventHandler(double[] phase);

    public class FourierOnlyPhaseExtractor : IPhaseExtractor {
        private readonly Rotator _rotator = new Rotator();
        private Complex[] _complexContainer;
        private double[] _phaseArray;


        public double[] GetPhase(double[] symmetryPulse, [CanBeNull] Complex[] correspondSpectrum) {
            if (_phaseArray == null) {
                _phaseArray = new double[symmetryPulse.Length/2];
            }

            Complex[] complexSpectrum;
            if (correspondSpectrum == null) {
                if (_complexContainer == null) {
                    _complexContainer = new Complex[symmetryPulse.Length];
                }
                Functions.ToComplexRotate(symmetryPulse, _complexContainer);
                Fourier.Forward(_complexContainer, FourierOptions.Matlab);
                complexSpectrum = _complexContainer;
            } else {
                complexSpectrum = correspondSpectrum;
            }

            RawSpectrumReady?.Invoke(complexSpectrum);

            //            symmetryPulse.ToComplex(_complexContainer);
            //            _rotator.Rotate(_complexContainer);

            // rotate & to complex

            for (var i = 0; i < _phaseArray.Length; i++) {
                _phaseArray[i] = complexSpectrum[i].Phase;
            }
            RawPhaseReady?.Invoke(_phaseArray);
            return _phaseArray;
        }

        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;
    }

    public class SpecifiedFreqRangePhaseExtractor : IPhaseExtractor {
        private readonly double _endFreqInM;
        private readonly double _samplingRateInM;
        private readonly double _startFreqInM;
        private SpecifiedRangePhaseExtractor _extractor;

        public SpecifiedFreqRangePhaseExtractor(double startFreqInM, double endFreqInM, double samplingRateInM) {
            _startFreqInM = startFreqInM;
            _endFreqInM = endFreqInM;
            _samplingRateInM = samplingRateInM;
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            if (_extractor == null) {
                _extractor = Init(symmetryPulse.Length);
            }
            return _extractor.GetPhase(symmetryPulse, correspondSpectrum);
        }

        public event SpectrumReadyEventHandler RawSpectrumReady {
            add { _extractor.RawSpectrumReady += value; }
            remove { _extractor.RawSpectrumReady -= value; }
        }

        public event PhaseReadyEventHandler RawPhaseReady {
            add { _extractor.RawPhaseReady += value; }
            remove { _extractor.RawPhaseReady -= value; }
        }

        [NotNull]
        private SpecifiedRangePhaseExtractor Init(int wholeFreqLength) {
            var factor = wholeFreqLength/_samplingRateInM;
            var startIndex = (int) (_startFreqInM*factor);
            var endIndex = (int) (_endFreqInM*factor);
            return new SpecifiedRangePhaseExtractor(startIndex, endIndex);
        }
    }

    public class PhaseFitException : Exception {
    }

    public class CorrectFailException : Exception {
    }

    public class SpecifiedRangePhaseExtractor : IPhaseExtractor {
        public enum DivisionResult {
            NoLeapPtsFound,
            AllIntervalTooShort,
            BestIntervalFound
        }

        private readonly int _end;
        private readonly double[] _linespace;
        private readonly int _rangeLength;
        private readonly double[] _rangePhaseContainer;
        private readonly Complex[] _rangeSpecContainer;

        private readonly int _start;

        private Complex[] _fullComplexContainer;
        private double[] _halfDoubleContainer;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpecifiedRangePhaseExtractor(int start, int end) {
            _start = start;
            _end = end;
            _rangeLength = end - start + 1;
            _linespace = Functions.LineSpace(start, end, _rangeLength);
            _rangeSpecContainer = new Complex[_rangeLength];
            _rangePhaseContainer = new double[_rangeLength];
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            if (_halfDoubleContainer == null || _fullComplexContainer == null) {
                _halfDoubleContainer = new double[symmetryPulse.Length/2];
                _fullComplexContainer = new Complex[symmetryPulse.Length];
            }

            Complex[] spectrum;
            if (correspondSpectrum == null) {
                if (_fullComplexContainer == null) {
                    _fullComplexContainer = new Complex[symmetryPulse.Length];
                }
                Functions.ToComplexRotate(symmetryPulse, _fullComplexContainer);
                _fullComplexContainer.FFT();
                spectrum = _fullComplexContainer;
            } else {
                spectrum = correspondSpectrum;
            }
            // todo including problem
            Array.Copy(spectrum, _start, _rangeSpecContainer, 0, _rangeLength);

            RawSpectrumReady?.Invoke(_rangeSpecContainer);

            _rangeSpecContainer.Phase(_rangePhaseContainer);
            Functions.UnwrapInPlace(_rangePhaseContainer);

            RawPhaseReady?.Invoke(_rangePhaseContainer);
            Tuple<double, double> tuple;
            Tuple<double, int, int> bestZone;
            var result = TryGetSmoothestInterval(_rangePhaseContainer, out bestZone);
            switch (result) {
                case DivisionResult.BestIntervalFound:
                    ThrowIfStdTooLarge(bestZone.Item1);
                    var start = bestZone.Item2;
                    var end = bestZone.Item3;
                    var lineSpace = Functions.LineSpace(start + _start, end + _start);
                    var doubles =
                        _rangePhaseContainer.Where((d, i) => i >= start && i <= end).ToArray();
                    tuple = Fit.Line(lineSpace, doubles);
                    break;
                case DivisionResult.NoLeapPtsFound:
                    var standardDeviation = _rangePhaseContainer.AsEnumerable().StandardDeviation();
                    ThrowIfStdTooLarge(standardDeviation);
                    tuple = Fit.Line(_linespace, _rangePhaseContainer);
                    break;
                case DivisionResult.AllIntervalTooShort:
                default:
                    throw new PhaseFitException();
            }
//            tuple = Fit.Line(_linespace, _rangePhaseContainer);
            var slope = tuple.Item2;
//             if (Math.Abs(slope)>0.05) {
//                 throw new PhaseFitException();
//             }
            var intersect = tuple.Item1;

            for (var i = 0; i < _halfDoubleContainer.Length; i++) {
                _halfDoubleContainer[i] = intersect + slope*i;
            }
            return _halfDoubleContainer;
        }


        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;

        private static void ThrowIfStdTooLarge(double std) {
            if (std > 0.34) {
                throw new PhaseFitException();
            }
        }

        public DivisionResult TryGetSmoothestInterval(double[] phase, out Tuple<double, int, int> bestZone) {
            var last = phase[0];

            var leapPts = new List<int>();
            for (var i = 1; i < phase.Length; i++) {
                var curr = phase[i];
                var delta = curr - last;
                if (Math.Abs(delta) > Math.PI*0.7) {
                    leapPts.Add(i);
                }
                last = curr;
            }
            if (leapPts.IsEmpty()) {
                bestZone = null;
                return DivisionResult.NoLeapPtsFound;
            }

            var intervals = new List<Tuple<int, int>> {new Tuple<int, int>(0, leapPts.First() - 1)};
            for (var i = 0; i < leapPts.Count - 1; i++) {
                var start = leapPts[i];
                var end = leapPts[i + 1] - 1;
                intervals.Add(new Tuple<int, int>(start, end));
            }
            intervals.Add(new Tuple<int, int>(leapPts.Last(), phase.Length - 1));
            intervals.RemoveAll(tuple => tuple.Item2 - tuple.Item1 <= 200);
            if (intervals.IsEmpty()) {
                bestZone = null;
                return DivisionResult.AllIntervalTooShort;
            }
            var leastStd = new Tuple<double, int, int>(double.MaxValue, -1, -1);
            intervals.ForEach(tuple => {
                var enumerable = _rangePhaseContainer.Where((d, j) => j >= tuple.Item1 && j <= tuple.Item2);
                var standardDeviation = enumerable.StandardDeviation();
                if (standardDeviation <= leastStd.Item1) {
                    leastStd = new Tuple<double, int, int>(standardDeviation, tuple.Item1, tuple.Item2);
                }
            });
            bestZone = leastStd;
            return DivisionResult.BestIntervalFound;
        }
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

    public class ClassicWrongPhaseExtractor : IPhaseExtractor {
        private readonly CentralInterpolationPhaseExtractor _phaseExtractor;

        public ClassicWrongPhaseExtractor(IApodizer apodizer, int centerHalfWidth) {
            _phaseExtractor = new CentralInterpolationPhaseExtractor(apodizer, centerHalfWidth, Functions.Phase);
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            return _phaseExtractor.GetPhase(symmetryPulse, correspondSpectrum);
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

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            return _phaseExtractor.GetPhase(symmetryPulse, correspondSpectrum);
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