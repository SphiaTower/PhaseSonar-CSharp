using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    public class PulseChecker {
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
        public PulseChecker([NotNull] ICrestFinder finder, ISlicer slicer, IPulsePreprocessor preprocessor,
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
        public IList<SpecInfo> Process([NotNull] double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return new List<SpecInfo>();
            }
            var sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            if (sliceInfos.IsEmpty()) {
                return new List<SpecInfo>();
            }

            return sliceInfos.Select(
                (sliceInfo, i) => {
                    var pulse = _preprocessor.RetrievePulse(pulseSequence, sliceInfo.StartIndex,
                        sliceInfo.CrestOffset,
                        sliceInfo.Length);
                    _rotator.TrySymmetrize(pulse, sliceInfo.CrestOffset); // todo do it at preproposs
                    var correctedSpectrum = _corrector.Correct(pulse);
                    return new SpecInfo(correctedSpectrum.Aggregate((complex, complex1) => complex+complex1)/correctedSpectrum.Length, i);
                }
                ).ToList();
        }
    }

    public class SpecInfo {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpecInfo(Complex first, int number) {
            First = first;
            Number = number;
        }

        public Complex First { get; }
        public int Number { get; }
    }
}

