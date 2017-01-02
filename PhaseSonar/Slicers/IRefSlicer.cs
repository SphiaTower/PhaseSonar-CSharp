using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace PhaseSonar.Slicers.RefSlicers {
    /// <summary>
    ///     A slicer which slices a pulse sequence and gets the start index of each pulse.
    /// </summary>
    public interface IRefSlicer {
        /// <summary>
        ///     Slice the pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <exception cref="SliceException"></exception>
        /// <returns>Start indices of pulses of different components, for example, gas and reference</returns>
        Duo<List<SliceInfo>> Slice([NotNull] double[] pulseSequence, [NotNull] IList<int> crestIndices);
    }
}