using JetBrains.Annotations;
using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers.WithoutReference {
    public class AccumulationResult : ProcessResult<ISpectrum> {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public AccumulationResult(ISpectrum data, ProcessException? exception, int exceptionCnt)
            : base(data, exception, exceptionCnt) {
        }


        [NotNull]
        public static AccumulationResult FromException(ProcessException exception, int cnt = 1) {
            return new AccumulationResult(null, exception, cnt);
        }

        [NotNull]
        public static AccumulationResult WithoutException(ISpectrum spectrum) {
            return new AccumulationResult(spectrum, null, 0);
        }
    }
}