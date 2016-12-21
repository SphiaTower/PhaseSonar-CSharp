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

    public enum ProcessException {
        NoPeakFound,
        NoSliceValid,
        NoFlatPhaseIntervalFound
    }

    public class AccumulationResult {
        public readonly int Cnt;
        public readonly ProcessException? Exception;
        public readonly ISpectrum Spectrum;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public AccumulationResult(ISpectrum spectrum, ProcessException? exception, int cnt) {
            Spectrum = spectrum;
            if (cnt!=0) {
                Exception = exception;
            }
            Cnt = cnt;
        }

        public bool HasSpectrum => Spectrum != null;
        public bool HasException => Exception != null;

        [NotNull]
        public static AccumulationResult FromException(ProcessException exception, int cnt = 1) {
            return new AccumulationResult(null, exception, cnt);
        }

        [NotNull]
        public static AccumulationResult WithoutException(ISpectrum spectrum) {
            return new AccumulationResult(spectrum, null, 0);
        }
    }

    public class SplitResult {
        public readonly ProcessException? Exception;

        public readonly int ExceptionCnt;
        public readonly GasRefTuple Spectrum;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SplitResult(GasRefTuple spectrum, ProcessException? exception, int exceptionCnt) {
            Spectrum = spectrum;
            if (exceptionCnt != 0) {
                Exception = exception;
            }
            ExceptionCnt = exceptionCnt;
        }

        public bool HasSpectrum => Spectrum != null;
        public bool HasException => Exception != null;

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