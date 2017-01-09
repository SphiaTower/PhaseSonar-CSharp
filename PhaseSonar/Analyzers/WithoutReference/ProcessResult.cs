namespace PhaseSonar.Analyzers.WithoutReference {
    /// <summary>
    /// A wrapper containing process results and exceptions
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ProcessResult<T> {
        public readonly T Data;
        public readonly ProcessException? Exception;
        public readonly int ExceptionCnt;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        protected ProcessResult(T data, ProcessException? exception, int exceptionCnt) {
            ExceptionCnt = exceptionCnt;
            if (exceptionCnt != 0) {
                Exception = exception;
            }
            Data = data;
        }

        public bool HasSpectrum => Data != null;
        public bool HasException => Exception != null;
    }
}