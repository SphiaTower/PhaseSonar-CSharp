using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers {
    public interface IRuler {
        int MeasureSliceLength([NotNull] IList<int> crestIndices, int fullLength);
    }

    public class FixLengtherRuler : IRuler {
        private readonly int _length;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public FixLengtherRuler(int length) {
            _length = length;
        }

        public int MeasureSliceLength(IList<int> crestIndices, int fullLength) {
            return _length;
        }
    }

    public class AverageLengthRuler : IRuler {
        public int MeasureSliceLength(IList<int> crestIndices, int fullLength) {
            if (crestIndices.Count==1) {
                return fullLength;
            }
            var average = 0;
            for (var i = 1; i < crestIndices.Count; i++) {
                var diff = crestIndices[i] - crestIndices[i - 1];
                average += diff;
            }
            return average/(crestIndices.Count - 1);
        }
    }

    public class MinCommonLengthRuler : IRuler {
        public int MeasureSliceLength(IList<int> crestIndices, int fullLength) {
            if (crestIndices.Count == 1) {
                return fullLength;
            }
            var min = int.MaxValue;
            for (var i = 1; i < crestIndices.Count; i++) {
                var diff = crestIndices[i] - crestIndices[i - 1];
                if (diff < min) {
                    min = diff;
                }
            }
            return min;
        }
    }
}