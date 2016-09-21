namespace PhaseSonar.Slicers {
    public interface IAligner {
        int CrestIndex(int minPtsCntBeforeCrest, int sliceLength);
    }

    public class LeftAligner : IAligner {
        public int CrestIndex(int minPtsCntBeforeCrest, int sliceLength) {
            return minPtsCntBeforeCrest;
        }
    }

    public class CenterAligner : IAligner {
        public int CrestIndex(int minPtsCntBeforeCrest, int sliceLength) {
            return sliceLength/2;
        }
    }
}