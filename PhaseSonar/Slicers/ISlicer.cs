using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Utils;

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

    public struct SliceInfo {
        public readonly int Length;
        public readonly int StartIndex;
        public readonly int CrestOffset;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public SliceInfo(int startIndex, int length, int crestOffset) {
            Length = length;
            CrestOffset = crestOffset;
            StartIndex = startIndex;
        }
    }
}