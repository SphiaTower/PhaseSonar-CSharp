using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers.Rulers {
    public interface IRuler {
        int MeasureSliceLength([NotNull] IList<int> crestIndices, int fullLength);
    }
}