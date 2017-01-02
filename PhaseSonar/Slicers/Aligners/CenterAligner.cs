namespace PhaseSonar.Slicers.Aligners {
    public class CenterAligner : IAligner {
        public int CrestIndex(int minPtsCntBeforeCrest, int sliceLength) {
            return sliceLength/2;
        }
    }
}