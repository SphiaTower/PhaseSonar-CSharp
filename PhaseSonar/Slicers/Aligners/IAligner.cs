namespace PhaseSonar.Slicers.Aligners {
    public interface IAligner {
        int CrestIndex(int minPtsCntBeforeCrest, int sliceLength);
    }
}