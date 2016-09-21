namespace PhaseSonar.Slicers {
    /* /// <summary>
     ///     A slicer for pulse sequences with 2 components, for example, gas and ref
     /// </summary>
     public class RefSlicer : SimpleSlicer {
         /// <summary>
         ///     Create a crest finder
         /// </summary>
         /// <param name="finder"></param>
         public RefSlicer(ICrestFinder finder) : base(123) {
         }

      /*   /// <summary>
         ///     Slice the pulse sequence, without considering multiple components.
         /// </summary>
         /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
         /// <returns>Whether slicing succeeded</returns>
         public override IList<IList<int>> Slice(double[] pulseSequence) {
             var crestIndices = Finder.Find(pulseSequence);
             if (crestIndices.NotEmpty()) {
                 var tuple = Group(crestIndices);
                 SliceLength = MinPeriodLength(crestIndices);
                 IList<int> startIndices1, startIndices2;

                 if (FindStartIndices(pulseSequence, tuple.Item1, SliceLength, out startIndices1) &&
                     FindStartIndices(pulseSequence, tuple.Item2, SliceLength, out startIndices2)) {
                     return new List<IList<int>>(2) {startIndices1, startIndices2};
                 }
             }
             return new List<IList<int>>(0);
         }#1#


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
     }*/
}