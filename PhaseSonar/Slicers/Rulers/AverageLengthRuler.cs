using System.Collections.Generic;

namespace PhaseSonar.Slicers.Rulers {
    public class AverageLengthRuler : IRuler {
        public int MeasureSliceLength(IList<int> crestIndices, int fullLength) {
            if (crestIndices.Count == 1) {
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
}