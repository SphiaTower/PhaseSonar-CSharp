using System.Linq;
using JetBrains.Annotations;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.PhaseExtractors;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers {
    public class PhaseReader : IPhaseReader {
        [NotNull] private readonly ICrestFinder _finder;

        [NotNull] private readonly IPhaseExtractor _phaseExtractor;

        [NotNull] private readonly IPulsePreprocessor _preprocessor;

        [NotNull] private readonly Rotator _rotator = new Rotator();

        [NotNull] private readonly ISlicer _slicer;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public PhaseReader(ICrestFinder finder, ISlicer slicer, IPulsePreprocessor preprocessor,
            IPhaseExtractor phaseExtractor) {
            _finder = finder;
            _slicer = slicer;
            _preprocessor = preprocessor;
            _phaseExtractor = phaseExtractor;
        }

        [NotNull]
        public PhaseResult GetPhase(double[] pulseSequence) {
            var crestIndices = _finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) {
                return PhaseResult.FromException(ProcessException.NoPeakFound);
            }
            var sliceInfos = _slicer.Slice(pulseSequence, crestIndices);
            if (sliceInfos.IsEmpty()) {
                return PhaseResult.FromException(ProcessException.NoSliceValid);
            }
            var example = sliceInfos.First();
            var pulse = _preprocessor.RetrievePulse(pulseSequence, example.StartIndex, example.CrestOffset,
                example.Length);
            _rotator.TrySymmetrize(pulse, example.CrestOffset);
            double[] phase;
            try {
                phase = _phaseExtractor.GetPhase(pulse, null);
            } catch (PhaseFitException) {
                return PhaseResult.FromException(ProcessException.NoFlatPhaseIntervalFound);
            }

            var unwrap = Functions.Unwrap(phase);

            return PhaseResult.WithoutException(unwrap);
        }
    }
}