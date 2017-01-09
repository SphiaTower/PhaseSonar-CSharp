using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.PhaseExtractors {
    public class SpecifiedFreqRangePhaseExtractor : IPhaseExtractor {
        private readonly double _endFreqInM;
        private readonly double _maxPhaseStd;
        private readonly int _minFlatPhasePtsNumCnt;
        private readonly double _samplingRateInM;
        private readonly double _startFreqInM;
        private SpecifiedRangePhaseExtractor _extractor;

        public SpecifiedFreqRangePhaseExtractor(double startFreqInM, double endFreqInM, double samplingRateInM,
            int minFlatPhasePtsNumCnt, double maxPhaseStd) {
            _startFreqInM = startFreqInM;
            _endFreqInM = endFreqInM;
            _samplingRateInM = samplingRateInM;
            _minFlatPhasePtsNumCnt = minFlatPhasePtsNumCnt;
            _maxPhaseStd = maxPhaseStd;
        }

        public double[] GetPhase(double[] symmetryPulse, Complex[] correspondSpectrum) {
            if (_extractor == null) {
                _extractor = Init(symmetryPulse.Length);
            }
            return _extractor.GetPhase(symmetryPulse, correspondSpectrum);
        }


        [NotNull]
        private SpecifiedRangePhaseExtractor Init(int wholeFreqLength) {
            var factor = wholeFreqLength/_samplingRateInM;
            var startIndex = (int) (_startFreqInM*factor);
            var endIndex = (int) (_endFreqInM*factor);
            return new SpecifiedRangePhaseExtractor(startIndex, endIndex, _minFlatPhasePtsNumCnt, _maxPhaseStd);
        }
    }
}