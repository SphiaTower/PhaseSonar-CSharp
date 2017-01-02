using JetBrains.Annotations;
using PhaseSonar.Analyzers.WithoutReference;

namespace PhaseSonar.Analyzers.WithReference {
    public class SplitResult : ProcessResult<GasRefTuple> {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SplitResult(GasRefTuple data, ProcessException? exception, int exceptionCnt)
            : base(data, exception, exceptionCnt) {
        }


        [NotNull]
        public static SplitResult FromException(ProcessException exception, int cnt = 1) {
            return new SplitResult(null, exception, cnt);
        }

        [NotNull]
        public static SplitResult WithoutException(GasRefTuple spectrum) {
            return new SplitResult(spectrum, null, 0);
        }
    }
}