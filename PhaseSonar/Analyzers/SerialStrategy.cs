using System.Collections.Generic;
using System.Linq;
using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     A serial strategy that process pulses one by one.
    /// </summary>
    public class SerialStrategy : IAnalyzerStrategy
    {
        private readonly ICorrector _corrector;

        /// <summary>
        ///     Create a serial strategy.
        /// </summary>
        /// <param name="corrector"></param>
        public SerialStrategy(ICorrector corrector)
        {
            _corrector = corrector;
        }


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
        public List<ISpectrum> Process(double[] pulseSequence, IList<IList<int>> startIndicesList, int pulseLength,
            int crestIndex)
        {
            _corrector.ClearBuffer(); //todo 1st time 
            return
                startIndicesList.Select(
                    startIndices => CorrectSequentially(pulseSequence, startIndices, pulseLength, crestIndex)).ToList();
        }

        private ISpectrum CorrectSequentially(double[] pulseSequence, IList<int> startIndices, int pulseLength,
            int crestIndex)
        {
            foreach (var startIndex in startIndices)
            {
                // todo: exception
                _corrector.Correct(pulseSequence, startIndex, pulseLength, crestIndex);
            }
            return _corrector.OutputSpetrumBuffer().Clone();
        }
    }
}