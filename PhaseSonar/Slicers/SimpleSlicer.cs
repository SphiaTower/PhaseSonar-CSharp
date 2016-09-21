using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PhaseSonar.CrestFinders;
using PhaseSonar.Utils;

namespace PhaseSonar.Slicers {
    /// <summary>
    ///     A basic slicer implementation which groups all slices into one list.
    /// </summary>
    public class SimpleSlicer : ISlicer {
        private readonly int _minPtsCntBeforeCrest;

        /// <summary>
        ///     Create a slicer.
        /// </summary>
        /// <param name="finder">
        ///     <see cref="ICrestFinder" />
        /// </param>
        public SimpleSlicer(int minPtsCntBeforeCrest, IRuler ruler, IAligner aligner) {
            _minPtsCntBeforeCrest = minPtsCntBeforeCrest;
            Ruler = ruler;
            Aligner = aligner;
        }

        [NotNull]
        protected IRuler Ruler { get; }

        public IAligner Aligner { get; set; }

        /// <summary>
        ///     Slice the pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <returns>Start indices of pulses of different components, for example, gas and reference</returns>
        [NotNull]
        public List<SliceInfo> Slice(double[] pulseSequence, IList<int> crestIndices) {
            if (crestIndices.IsEmpty()) return new List<SliceInfo>(0);
            var sliceLength = Ruler.MeasureSliceLength(crestIndices);
            var crestOffset = Aligner.CrestIndex(_minPtsCntBeforeCrest, sliceLength);

            IList<int> startIndices;
            return FindStartIndices(pulseSequence, crestIndices, sliceLength, crestOffset, out startIndices)
                ? startIndices.Select(index => new SliceInfo(index, sliceLength, crestOffset)).ToList()
                : new List<SliceInfo>(0);
        }

        /// <summary>
        ///     Map crest indices to start indices.
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence which contains all the pulses.</param>
        /// <param name="crestIndices">The indices of all crests.</param>
        /// <param name="periodLength">The common pulse length.</param>
        /// <returns>The start indices for all the slices.</returns>
        protected virtual bool FindStartIndices([NotNull] double[] pulseSequence, [NotNull] IList<int> crestIndices,
            int periodLength, int crestOffset, [NotNull] out IList<int> startIndices) {
            var length = pulseSequence.Length;
            startIndices = crestIndices; // todo deep clone
            for (var i = 0; i < crestIndices.Count; i++) {
                crestIndices[i] -= crestOffset;
            }
            // todo SliceFailedException
            if (crestIndices[0] < 0) {
                crestIndices.RemoveAt(0);
            }
            if (crestIndices.Count == 0) {
                return false;
            }
            if (crestIndices.Last() + periodLength >= length) {
                crestIndices.RemoveAt(crestIndices.Count - 1);
            }
            return crestIndices.Count != 0;
        }
    }
}