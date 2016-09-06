using JetBrains.Annotations;

namespace PhaseSonar.Correctors {
    /// <summary>
    ///     A corrector that corrects the phase of the temporal data of one pulse.
    /// </summary>
    public interface ICorrector {
        /// <summary>
        ///     The length of the output
        /// </summary>
        int OutputLength { get; }

        /// <summary>
        ///     Get the buffer which stores the latest output
        /// </summary>
        /// <returns>The buffer which stores the latest output</returns>
        [NotNull]
        ISpectrum OutputSpetrumBuffer();

        /// <summary>
        ///     Correct a pulse
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence that the pulse contains in</param>
        /// <param name="startIndex">The start index of the pulse in the pulse sequence</param>
        /// <param name="pulseLength">The length of the pulse</param>
        /// <param name="crestIndex">The number of points before the crest</param>
        void Correct([NotNull] double[] pulseSequence, int startIndex, int pulseLength, int crestIndex);

        /// <summary>
        ///     Reset the status of the corrector
        /// </summary>
        void ClearBuffer();
    }
}