using System.Collections.Generic;
using JetBrains.Annotations;

namespace FTIR.Slicers
{
    public interface ISlicer
    {
        int SlicedPeriodLength { get; set; }
        int SliceStartOffset { get; }
        [CanBeNull]
        List<List<int>> Slice([NotNull]double[] pulseSequence);
    }
}