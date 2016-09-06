using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers {
    /// <summary>
    ///     A finder that identifies crests in a pulse sequence.
    /// </summary>
    public interface ICrestFinder {
        /// <summary>
        ///     The minimum number of points that is before the crest.
        /// </summary>
        int LeftThreshold { get; }

        /// <summary>
        ///     Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The indices of the crests</returns>
        IList<int> Find([NotNull] double[] pulseSequence);
    }
}