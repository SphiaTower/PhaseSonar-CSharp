using JetBrains.Annotations;
using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     An basic analyser which slices and processes the pulse sequence.
    /// </summary>
    public interface IPulseSequenceProcessor {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        AccumulationResult Process([NotNull] double[] pulseSequence);
    }

    public interface IRefPulseSequenceProcessor {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        SplitResult Process([NotNull] double[] pulseSequence);
    }

    public interface IPhaseReader {
        PhaseResult GetPhase([NotNull] double[] pulseSequence);
    }

    public enum ProcessException {
        NoPeakFound,
        NoSliceValid,
        NoFlatPhaseIntervalFound
    }

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