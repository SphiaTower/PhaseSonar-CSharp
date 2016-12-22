using System;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.PhaseExtractors;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    public class Splitter : IRefPulseSequenceProcessor {
        [NotNull] private readonly ICorrectorV2 _corrector;

        [NotNull] private readonly ICrestFinder _finder;

        [NotNull] private readonly IPulsePreprocessor _preprocessor;

        [NotNull] private readonly Rotator _rotator = new Rotator();

        [NotNull] private readonly IRefSlicer _slicer;

        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="finder">A finder</param>
        /// <param name="slicer">A slicer</param>
        /// <param name="preprocessor"></param>
        /// <param name="corrector">A corrector</param>
        public Splitter([NotNull] ICrestFinder finder, IRefSlicer slicer, IPulsePreprocessor preprocessor,
            ICorrectorV2 corrector) {
            _preprocessor = preprocessor;
            _corrector = corrector;
            _finder = finder;
            _slicer = slicer;
        }


        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        public SplitResult Process([NotNull] double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.Count <= 1) {
                return SplitResult.FromException(ProcessException.NoPeakFound);
            }
            Duo<List<SliceInfo>> sliceInfos;
            try {
                sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            } catch (Exception) {
                return SplitResult.FromException(ProcessException.NoSliceValid);
            }
            var errorCnt = 0;
            var spectra = new Spectrum[2];
            for (var i = 0; i < 2; i++) {
                var list = sliceInfos[i];
                Complex[] accumulatedSpectrum = null;
                var cnt = 0;

                foreach (var sliceInfo in list) {
                    var pulse = _preprocessor.RetrievePulse(pulseSequence, sliceInfo.StartIndex,
                        sliceInfo.CrestOffset,
                        sliceInfo.Length);
                    _rotator.TrySymmetrize(pulse, sliceInfo.CrestOffset); // todo do it at preproposs
                    Complex[] correctedSpectrum;
                    try {
                        correctedSpectrum = _corrector.Correct(pulse);
                    } catch (CorrectFailException) {
                        errorCnt++;
                        continue;
                    }
                    if (accumulatedSpectrum == null) {
                        accumulatedSpectrum = correctedSpectrum.Clone() as Complex[];
                    } else {
                        accumulatedSpectrum.Increase(correctedSpectrum);
                    }
                    cnt++;
                }
                if (accumulatedSpectrum == null) {
                    return SplitResult.FromException(ProcessException.NoFlatPhaseIntervalFound, errorCnt);
                }
                spectra[i] = new Spectrum(accumulatedSpectrum, cnt);
            }

            var duo = Duo.Create(spectra[0], spectra[1]);
            GasRefTuple tuple;
            if (Sum(duo.Item2.Array) > Sum(duo.Item1.Array)) {
                tuple = GasRefTuple.SourceAndRef(duo.Item2, duo.Item1);
            } else {
                tuple = GasRefTuple.SourceAndRef(duo.Item1, duo.Item2);
            }
            if (errorCnt != 0) {
                return new SplitResult(tuple, ProcessException.NoFlatPhaseIntervalFound, errorCnt);
            }
            return SplitResult.WithoutException(tuple);
        }

        private static double Sum(Complex[] array) {
            double sum = 0;
            for (var i = 0; i < array.Length/2; i++) {
                sum += array[i].Magnitude;
            }
            return sum;
        }
    }
}