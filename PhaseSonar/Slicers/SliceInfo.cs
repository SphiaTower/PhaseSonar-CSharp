namespace PhaseSonar.Slicers {
    public struct SliceInfo {
        public readonly int Length;
        public readonly int StartIndex;
        public readonly int CrestOffset;

        /// <summary>初始化 <see cref="T:System.Object" /> 类的新实例。</summary>
        public SliceInfo(int startIndex, int length, int crestOffset) {
            Length = length;
            CrestOffset = crestOffset;
            StartIndex = startIndex;
        }
    }
}