namespace PhaseSonar.Slicers {
    public struct SliceInfo {
        public readonly int Length;
        public readonly int StartIndex;
        public readonly int CrestOffset;

        /// <summary>��ʼ�� <see cref="T:System.Object" /> �����ʵ����</summary>
        public SliceInfo(int startIndex, int length, int crestOffset) {
            Length = length;
            CrestOffset = crestOffset;
            StartIndex = startIndex;
        }
    }
}