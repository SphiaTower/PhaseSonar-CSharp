using JetBrains.Annotations;
using PhaseSonar.Analyzers.WithoutReference;

namespace PhaseSonar.Analyzers.PhaseAnalyzers {
    public class PhaseResult : ProcessResult<double[]> {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public PhaseResult(double[] data, ProcessException? exception, int exceptionCnt)
            : base(data, exception, exceptionCnt) {
        }

        [NotNull]
        public static PhaseResult FromException(ProcessException exception, int cnt = 1) {
            return new PhaseResult(null, exception, cnt);
        }

        [NotNull]
        public static PhaseResult WithoutException(double[] phase) {
            return new PhaseResult(phase, null, 0);
        }
    }
}