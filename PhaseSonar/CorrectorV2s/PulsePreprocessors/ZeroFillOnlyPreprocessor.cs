using System;
using JetBrains.Annotations;
using PhaseSonar.Maths;

namespace PhaseSonar.CorrectorV2s.PulsePreprocessors {
    public class ZeroFillOnlyPreprocessor : IPulsePreprocessor {
        private readonly int _zeroFillFactor;

        [CanBeNull] private double[] _zeroFilledSpace;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public ZeroFillOnlyPreprocessor(int zeroFillFactor) {
            _zeroFillFactor = zeroFillFactor;
        }


        public double[] RetrievePulse([NotNull] double[] pulseSequence, int startIndex, int crestIndexOffStart,
            int pulseLength) {
            if (_zeroFilledSpace == null) {
                _zeroFilledSpace = new double[Functions.CalZeroFilledLength(pulseLength, _zeroFillFactor)];
            }
            var i = 0;
            var safeLength = Math.Min(pulseLength, _zeroFilledSpace.Length);
            for (; i < safeLength; i++, startIndex++) {
                _zeroFilledSpace[i] = pulseSequence[startIndex];
            }
            for (; i < _zeroFilledSpace.Length; i++) {
                _zeroFilledSpace[i] = 0;
            }
            return _zeroFilledSpace;
        }
    }
}