using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers {
    /// <summary>
    ///     A slicer which slices a pulse sequence and gets the start index of each pulse.
    /// </summary>
    public interface ISlicer {
        /// <summary>
        ///     Slice the pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <returns>Start indices of pulses of different components, for example, gas and reference</returns>
        List<SliceInfo> Slice([NotNull] double[] pulseSequence, [NotNull] IList<int> crestIndices);
    }
}