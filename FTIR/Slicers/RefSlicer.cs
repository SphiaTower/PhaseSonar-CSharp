using System;
using System.Collections.Generic;
using System.Linq;
using FTIR.Correctors;

namespace FTIR.Slicers
{
    public class RefSlicer : SimpleSlicer
    {
       

        public override List<List<int>> Slice(double[] pulseSequence)
        {
            // TODO: adjust threshold
            var crestIndices = Finder.Find(pulseSequence);
            if (crestIndices == null)
            {
                return null;
            }
            var tuple = Group(crestIndices);
            SlicedPeriodLength = AnalyzePeriodLength(crestIndices);
            var startIndices1 = FindStartIndices(pulseSequence, tuple.Item1, SlicedPeriodLength);
            var startIndices2 = FindStartIndices(pulseSequence, tuple.Item2, SlicedPeriodLength);
            if (startIndices1==null||startIndices2==null)
            {
                return null;
            }
            return new List<List<int>>(2) {startIndices1, startIndices2};
        }

        private static Tuple<List<int>, List<int>> Group(List<int> crestIndices)
        {
            var group1 = new List<int>();
            var group2 = new List<int>();
            var periodLength = crestIndices[2] - crestIndices[0];
            var firstIndex = crestIndices[0];
            var secondIndex = crestIndices[1];
            foreach (var crest in crestIndices)
            {
                var threshold = periodLength/1.7;
                if (Near(crest, firstIndex, periodLength))
                {
                    CheckAdd(group1, crest, threshold);
                }
                else if (Near(crest, secondIndex, periodLength))
                {
                    CheckAdd(group2, crest, threshold);
                }
            }
            return new Tuple<List<int>, List<int>>(group1, group2);
        }

        private static void CheckAdd(ICollection<int> grp, int crest, double threshold)
        {
            if (grp.Count > 0)
            {
                if (crest - grp.Last() > threshold)
                {
                    grp.Add(crest);
                }
            }
            else
            {
                grp.Add(crest);
            }
        }

        private static bool Near(int crestIndex, int firstIndex, int periodLength, double range = 0.1)
        {
            var distance = crestIndex - firstIndex;
            var ratio = (double) distance/periodLength;
            return Math.Abs(ratio - Math.Round(ratio)) < range;
        }

        public RefSlicer(CrestFinder crestFinder) : base(crestFinder)
        {
        }
    }
}