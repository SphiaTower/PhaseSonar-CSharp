using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers
{
    /// <summary>
    /// A slicer which slices a pulse sequence and gets the start index of each pulse.
    /// </summary>
    public interface ISlicer
    {
        /// <summary>
        /// The pulse length after sliced.
        /// </summary>
        int SlicedPeriodLength { get; set; }
        /// <summary>
        /// The index or offset of the crest relative to the start index.
        /// </summary>
        int CrestIndex { get; }

        /// <summary>
        /// Slice the pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <param name="startIndicesList">Start indices of pulses of different components, for example, gas and reference</param>
        /// <returns>Whether slicing succeeded</returns>
        bool Slice([NotNull]double[] pulseSequence,out IList<IList<int>> startIndicesList);
    }
}