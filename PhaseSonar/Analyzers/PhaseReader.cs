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
    public class PhaseReader :IPulseSequenceProcessor {

        [NotNull]
        private readonly ICrestFinder _finder;

        [NotNull]
        private readonly IPulsePreprocessor _preprocessor;

        [NotNull]
        private readonly ISlicer _slicer;

        [NotNull] private readonly IPhaseExtractor _phaseExtractor;

        [NotNull] private readonly Rotator _rotator= new Rotator();

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public PhaseReader(ICrestFinder finder,ISlicer slicer, IPulsePreprocessor preprocessor, IPhaseExtractor phaseExtractor) {
            _finder = finder;
            _slicer = slicer;
            _preprocessor = preprocessor;
            _phaseExtractor = phaseExtractor;
        }

        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        public Maybe<ISpectrum> Process(double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return Maybe<ISpectrum>.Empty();
            }
            var sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            if (sliceInfos.IsEmpty()) {
                return Maybe<ISpectrum>.Empty();
            }
            var example = sliceInfos.First();
            var pulse = _preprocessor.RetrievePulse(pulseSequence, example.StartIndex, example.CrestOffset, example.Length);
            _rotator.TrySymmetrize(pulse, example.CrestOffset);
            double[] phase = _phaseExtractor.GetPhase(pulse);
            var unwrap = Functions.Unwrap(phase);
            ISpectrum spectrum = new RealSpectrum(unwrap,1);
            return Maybe<ISpectrum>.Of(spectrum);
        }
    }
}