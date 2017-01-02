using System.Collections.Generic;

namespace PhaseSonar.Slicers.Rulers {
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
}