using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    public class Splitter:IRefPulseSequenceProcessor {
 
        [NotNull]
        private readonly ICorrectorV2 _corrector;

        [NotNull]
        private readonly ICrestFinder _finder;

        [NotNull]
        private readonly IPulsePreprocessor _preprocessor;
        [NotNull]
        private readonly Rotator _rotator = new Rotator();

        [NotNull]
        private readonly IRefSlicer _slicer;

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
        public Maybe<SplitResult> Process([NotNull] double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return Maybe<SplitResult>.Empty();
            }
            Duo<List<SliceInfo>> sliceInfos;
            try {
                sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            } catch (Exception) {
                return Maybe<SplitResult>.Empty();
            }
            try {
                var duo = sliceInfos.Select(list => AccumulatedSpectrum(pulseSequence, list)).ToDuo();
                for (var i = 0; i < duo.Count; i++) {
                    var spectrum = duo[i];
                    Toolbox.WriteData(@"D:\zbf\gas\ac"+i+".txt", spectrum.Array.Select(complex => complex.Magnitude).ToArray());
                }
                if (Sum(duo.Item2.Array)>Sum(duo.Item1.Array)) {
                    return Maybe<SplitResult>.Of(SplitResult.SourceAndRef(duo.Item2,duo.Item1));
                } else {
                    return Maybe<SplitResult>.Of(SplitResult.SourceAndRef(duo.Item1,duo.Item2));
                }
            } catch (NoPeriodAvailableException) {
                return Maybe<SplitResult>.Empty();
            }
        }

        private static double Sum(Complex[] array) {
            double sum = 0;
            for (var i = 0; i < array.Length/2; i++) {
                sum += array[i].Magnitude;
            }
            return sum;
        }

        [NotNull]
        private ISpectrum AccumulatedSpectrum([NotNull] double[] pulseSequence, [NotNull] IEnumerable<SliceInfo> sliceInfos) {
            Complex[] accumulatedSpectrum = null;
            int cnt = 0;

            foreach (var sliceInfo in sliceInfos) {
                var pulse = _preprocessor.RetrievePulse(pulseSequence, sliceInfo.StartIndex,
                    sliceInfo.CrestOffset,
                    sliceInfo.Length);
                //                Toolbox.WriteData(@"D:\zbf\temp\0_zero_filled.txt", pulse);
                _rotator.TrySymmetrize(pulse, sliceInfo.CrestOffset); // todo do it at preproposs
                Complex[] correctedSpectrum;
                try {
                    correctedSpectrum = _corrector.Correct(pulse);
                } catch (CorrectFailException) {
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
            if (accumulatedSpectrum==null) {
                throw new NoPeriodAvailableException();
            } else {
                return new Spectrum(accumulatedSpectrum,cnt);

            }
        }

        class NoPeriodAvailableException :Exception {
            
        }
    }
}
