using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;

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
        Maybe<ISpectrum> Process([NotNull] double[] pulseSequence);
    }

    public interface IRefPulseSequenceProcessor {
        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, often a sampled data record</param>
        /// <returns>The accumulated spectrum</returns>
        Maybe<SplitResult> Process([NotNull] double[] pulseSequence);
    }

    /*  public class SplitResult {
        public readonly ISpectrum Signal;
        public readonly ISpectrum Reference;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SplitResult(ISpectrum signal, ISpectrum reference) {
            Signal = signal;
            Reference = reference;
        }
    }*/
}