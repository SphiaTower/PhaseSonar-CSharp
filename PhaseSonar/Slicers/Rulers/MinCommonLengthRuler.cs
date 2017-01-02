using System.Collections.Generic;

namespace PhaseSonar.Slicers.Rulers {
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