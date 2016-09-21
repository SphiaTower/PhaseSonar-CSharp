using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     An analyzer which adds up all results in a pulse sequence.
    ///     This class is targeted for the data with 1 component only.
    /// </summary>
    public class Accumulator : IPulseSequenceProcessor {
        private readonly ICorrectorV2 _corrector;

        [NotNull] private readonly ICrestFinder _finder;

        [NotNull] private readonly IPulsePreprocessor _preprocessor;

        [NotNull] private readonly ISlicer _slicer;

        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="slicer">A slicer</param>
        /// <param name="preprocessor"></param>
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
        public Maybe<ISpectrum> Accumulate([NotNull] double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return Maybe<ISpectrum>.Empty();
            }
            var sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            if (sliceInfos.IsEmpty()) {
                return Maybe<ISpectrum>.Empty();
            }

            var cnt = 0;
            Complex[] accumulatedSpectrum = null;
            foreach (var sliceInfo in sliceInfos) {
                var pulse = _preprocessor.RetrievePulse(pulseSequence, sliceInfo.StartIndex,
                    sliceInfo.CrestOffset,
                    sliceInfo.Length);
                var correctedSpectrum = _corrector.Correct(pulse, sliceInfo.CrestOffset);
                if (accumulatedSpectrum == null) {
                    accumulatedSpectrum = correctedSpectrum.Clone() as Complex[];
                } else {
                    accumulatedSpectrum.Increase(correctedSpectrum);
                }
                cnt++;
            }

            return Maybe<ISpectrum>.Of(new Spectrum(accumulatedSpectrum, cnt));
        }
    }
}