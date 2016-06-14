using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     The strategy which a analyer takes. See the strategy pattern.
    /// </summary>
    public interface IAnalyzerStrategy
    {
        /// <summary>
        ///     Process the pulse sequence, including slicing, phase correction, and accumulation.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence.</param>
        /// <param name="startIndicesList">
        ///     The list of start indices for pulses of different sources, for example, gas and
        ///     reference.
        /// </param>
        /// <param name="pulseLength">The length of every pulse</param>
        /// <param name="crestIndex">The index of the crest in the pulse</param>
        /// <returns>The processed result of the pulse sequence, or null if failed</returns>
        [CanBeNull]
        List<ISpectrum> Process(double[] pulseSequence, [NotNull] List<List<int>> startIndicesList, int pulseLength,
            int crestIndex);
    }
}