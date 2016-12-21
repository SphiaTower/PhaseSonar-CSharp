using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace PhaseSonar.Slicers {
    /// <summary>
    ///     A slicer for pulse sequences with 2 components, for example, gas and ref
    /// </summary>
    public class RefSlicer : IRefSlicer {
        private readonly int _minPtsCntBeforeCrest;

        /// <summary>
        ///     Create a crest finder
        /// </summary>
        /// <param name="finder"></param>
        public RefSlicer(int minPtsCntBeforeCrest, IRuler ruler, IAligner aligner) {
            Ruler = ruler;
            _minPtsCntBeforeCrest = minPtsCntBeforeCrest;
            Aligner = aligner;
        }

        public IRuler Ruler { get; set; }
        public IAligner Aligner { get; set; }

        /// <summary>
        ///     Slice the pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <exception cref="SliceException"></exception>
        /// <returns>Start indices of pulses of different components, for example, gas and reference</returns>
        [NotNull]
        public Duo<List<SliceInfo>> Slice(double[] pulseSequence, IList<int> crestIndices) {
            if (crestIndices.Count<=1) throw new SliceException();
            var tuple = Group(crestIndices);
            var sliceLength = Ruler.MeasureSliceLength(crestIndices,pulseSequence.Length);
            IList<int> startIndices1, startIndices2;

            var crestOffset = Aligner.CrestIndex(_minPtsCntBeforeCrest, sliceLength);
            if (
                SimpleSlicer.FindStartIndices(pulseSequence, tuple.Item1, sliceLength, crestOffset, out startIndices1) &&
                SimpleSlicer.FindStartIndices(pulseSequence, tuple.Item2, sliceLength, crestOffset, out startIndices2)) {
                return Duo.Create(startIndices1, startIndices2)
                    .Select(ints => ints.Select(i => new SliceInfo(i, sliceLength, crestOffset)).ToList())
                    .ToDuo();
            }
            throw new SliceException();
        }


        private static Tuple<List<int>, List<int>> Group(IList<int> crestIndices) {
            var group1 = new List<int>();
            var group2 = new List<int>();
            var periodLength = crestIndices[2] - crestIndices[0];
            var firstIndex = crestIndices[0];
            var secondIndex = crestIndices[1];
            foreach (var crest in crestIndices) {
                var threshold = periodLength/1.7;
                if (Near(crest, firstIndex, periodLength)) {
                    CheckAdd(group1, crest, threshold);
                } else if (Near(crest, secondIndex, periodLength)) {
                    CheckAdd(group2, crest, threshold);
                }
            }
            return new Tuple<List<int>, List<int>>(group1, group2);
        }

        private static void CheckAdd(ICollection<int> grp, int crest, double threshold) {
            if (grp.Count > 0) {
                if (crest - grp.Last() > threshold) {
                    grp.Add(crest);
                }
            } else {
                grp.Add(crest);
            }
        }

        private static bool Near(int crestIndex, int firstIndex, int periodLength, double range = 0.1) {
            var distance = crestIndex - firstIndex;
            var ratio = (double) distance/periodLength;
            return Math.Abs(ratio - Math.Round(ratio)) < range;
        }
    }

    public class SliceException : Exception {
    }
}