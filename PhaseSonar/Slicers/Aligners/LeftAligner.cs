namespace PhaseSonar.Slicers.Aligners {
    public class LeftAligner : IAligner {
        public int CrestIndex(int minPtsCntBeforeCrest, int sliceLength) {
            return minPtsCntBeforeCrest;
        }
    }
}