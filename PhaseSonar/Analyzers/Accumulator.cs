using System;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     An analyzer which adds up all results in a pulse sequence.
    ///     This class is targeted for the data with 1 component only.
    /// </summary>
    public class Accumulator : IPulseSequenceProcessor {
        [NotNull] private readonly ICorrectorV2 _corrector;

        [NotNull] private readonly ICrestFinder _finder;

        [NotNull] private readonly IPulsePreprocessor _preprocessor;
        [NotNull] private readonly Rotator _rotator = new Rotator();

        [NotNull] private readonly ISlicer _slicer;

        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="finder">A finder</param>
        /// <param name="slicer">A slicer</param>
        /// <param name="preprocessor"></param>
        /// <param name="corrector">A corrector</param>
        public Accumulator([NotNull] ICrestFinder finder, ISlicer slicer, IPulsePreprocessor preprocessor,
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
        [NotNull]
        public AccumulationResult Process([NotNull] double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return AccumulationResult.FromException(ProcessException.NoPeakFound);
            }
            var sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            if (sliceInfos.IsEmpty()) {
                return AccumulationResult.FromException(ProcessException.NoSliceValid);
            }

            var cnt = 0;
            Complex[] accumulatedSpectrum = null;
            int errorCnt = 0;
            foreach (var sliceInfo in sliceInfos) {
                var pulse = _preprocessor.RetrievePulse(pulseSequence, sliceInfo.StartIndex,
                    sliceInfo.CrestOffset,
                    sliceInfo.Length);
//                Toolbox.WriteData(@"D:\zbf\temp\0_zero_filled.txt", pulse);
                _rotator.TrySymmetrize(pulse, sliceInfo.CrestOffset); // todo do it at preproposs
                Complex[] correctedSpectrum;
                try {
                    correctedSpectrum = _corrector.Correct(pulse);
                } catch (CorrectFailException e) {
                    errorCnt++;
                    continue;
                }
//                Toolbox.WriteData(@"D:\zbf\temp\sp.txt", correctedSpectrum);
                if (accumulatedSpectrum == null) {
                    accumulatedSpectrum = correctedSpectrum.Clone() as Complex[];
                } else {
                    accumulatedSpectrum.Increase(correctedSpectrum);
                }
                cnt++;
            }
            if (accumulatedSpectrum == null) {
                return AccumulationResult.FromException(ProcessException.NoFlatPhaseIntervalFound,errorCnt);
            } else {
                var spectrum = new Spectrum(accumulatedSpectrum,cnt);
                if (errorCnt==0) {
                    return AccumulationResult.WithoutException(spectrum);
                } else {
                    return new AccumulationResult(spectrum,ProcessException.NoFlatPhaseIntervalFound, errorCnt);
                }
            }
        }
    }

    class NoPeakFoundException : Exception {
        
    }

    class SliceFailedExceptin : Exception {
        
    }

    class ExcessivePhaseLeapsException : Exception {
        
    }
}