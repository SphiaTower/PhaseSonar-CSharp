using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers
{
    /// <summary>
    /// A finder that identifies crests in a pulse sequence.
    /// </summary>
    public interface ICrestFinder
    {
        /// <summary>
        /// The minimum number of points that is before the crest.
        /// </summary>
        int LeftThreshold { get; }

        /// <summary>
        /// Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <param name="crestIndices">The indices of crests</param>
        /// <returns>Whether crests are found successfully</returns>
        bool Find([NotNull]double[] pulseSequence,out IList<int> crestIndices);
    }
}