using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace PhaseSonar.Slicers
{
    /// <summary>
    ///     A basic slicer implementation which groups all slices into one list.
    /// </summary>
    public class SimpleSlicer : ISlicer
    {
        /// <summary>
        ///     Create a slicer.
        /// </summary>
        /// <param name="finder">
        ///     <see cref="ICrestFinder" />
        /// </param>
        public SimpleSlicer(ICrestFinder finder)
        {
            Finder = finder;

            // datalength/sampleRate*repetitionRate=pn
        }

        /// <summary>
        ///     <see cref="ICrestFinder" />
        /// </summary>
        protected ICrestFinder Finder { get; }

        /// <summary>
        ///     The index of the crest in the slice.
        /// </summary>
        public virtual int CrestIndex => Finder.LeftThreshold;

        /// <summary>
        ///     Slice the pulse sequence, without considering multiple components.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
        /// <returns>Whether slicing succeeded</returns>
        public virtual IList<IList<int>> Slice(double[] pulseSequence)
        {
            var startIndicesList = new List<IList<int>>(1);
            var crestIndices = Finder.Find(pulseSequence);
            if (crestIndices.IsEmpty()) return startIndicesList;
            SlicedPeriodLength = AnalyzePeriodLength(crestIndices);
            IList<int> startIndices;
            if (FindStartIndices(pulseSequence, crestIndices, SlicedPeriodLength, out startIndices))
            {
                startIndicesList.Add(startIndices);
            }
            return startIndicesList;
        }


        /// <summary>
        ///     The pulse length after sliced.
        /// </summary>
        public int SlicedPeriodLength { get; set; }


        /// <summary>
        ///     Get a common length for all the pulses.
        /// </summary>
        /// <param name="crestIndices">The indices of crests.</param>
        /// <returns>The minimum of all the intervals.</returns>
        protected static int AnalyzePeriodLength([NotNull] IList<int> crestIndices)
        {
            var min = int.MaxValue;
            for (var i = 1; i < crestIndices.Count; i++)
            {
                var diff = crestIndices[i] - crestIndices[i - 1];
                if (diff < min)
                {
                    min = diff;
                }
            }
            return min;
        }

        /// <summary>
        ///     Map crest indices to start indices.
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence which contains all the pulses.</param>
        /// <param name="crestIndices">The indices of all crests.</param>
        /// <param name="periodLength">The common pulse length.</param>
        /// <returns>The start indices for all the slices.</returns>
        protected virtual bool FindStartIndices([NotNull] double[] pulseSequence, [NotNull] IList<int> crestIndices,
            int periodLength, out IList<int> startIndices)
        {
            var length = pulseSequence.Length;
            startIndices = crestIndices; // todo deep clone
            for (var i = 0; i < crestIndices.Count; i++)
            {
                crestIndices[i] -= CrestIndex;
            }
            // todo SliceFailedException
            if (crestIndices[0] < 0)
            {
                crestIndices.RemoveAt(0);
            }
            if (crestIndices.Count == 0)
            {
                return false;
            }
            if (crestIndices.Last() + periodLength >= length)
            {
                crestIndices.RemoveAt(crestIndices.Count - 1);
            }
            return crestIndices.Count != 0;
        }
    }
}