using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.CrestFinders {
    /// <summary>
    ///     A finder that identifies crests in a pulse sequence.
    /// </summary>
    public interface ICrestFinder {
        /// <summary>
        ///     The minimum number of points that is before the crest.
        /// </summary>
        int MinPtsCntBeforeCrest { get; }

        double VerticalThreshold { get; set; }

        double RepetitionRate { get; }

        double SampleRate { get; }

        /// <summary>
        ///     Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The indices of the crests</returns>
        [NotNull]
        IList<int> Find([NotNull] double[] pulseSequence);
    }
}