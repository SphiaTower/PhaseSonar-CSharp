using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers
{
    public class SimpleSlicer : ISlicer
    {

        
        public SimpleSlicer(ICrestFinder crestFinder)
        {
            Finder = crestFinder;

            // datalength/sampleRate*repetitionRate=pn
        }

        protected ICrestFinder Finder { get; }
        public virtual int SliceStartOffset => Finder.LeftThreshold;

        public virtual List<List<int>> Slice([NotNull]double[] pulseSequence)
        {
            var crestIndices = Finder.Find(pulseSequence);
            if (crestIndices==null)
            {
                return null;
            }
            else
            {
                SlicedPeriodLength = AnalyzePeriodLength(crestIndices);
                var startIndices = FindStartIndices(pulseSequence, crestIndices, SlicedPeriodLength);
                return startIndices==null ? null : new List<List<int>>(1) {startIndices};
            }
        }


        public int SlicedPeriodLength { get; set; }


        protected static int AnalyzePeriodLength([NotNull]IList<int> crestIndices)
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
        [CanBeNull]
        protected virtual List<int> FindStartIndices([NotNull]double[] pulseSequence, [NotNull]List<int> crestIndices, int periodLength)
        {
            var length = pulseSequence.Length;
            for (var i = 0; i < crestIndices.Count; i++)
            {
                crestIndices[i] -= SliceStartOffset;
            }
            // todo SliceFailedException
            if (crestIndices[0] < 0)
            {
                crestIndices.RemoveAt(0);
            }
            if (crestIndices.Count==0)
            {
                return null;
            }
            if (crestIndices.Last() + periodLength >= length)
            {
                crestIndices.RemoveAt(crestIndices.Count - 1);
            }
            return crestIndices.Count==0 ? null : crestIndices;
        }
    }
}