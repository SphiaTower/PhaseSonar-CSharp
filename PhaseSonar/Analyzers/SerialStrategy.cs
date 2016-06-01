using System.Collections.Generic;
using System.Linq;
using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    /// A serial strategy that process pulses one by one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerialStrategy<T> : IAnalyzerStrategy<T> where T : ISpectrum
    {
        private readonly ICorrector<T> _corrector;

        /// <summary>
        /// Create a serial strategy.
        /// </summary>
        /// <param name="corrector"></param>
        public SerialStrategy(ICorrector<T> corrector)
        {
            _corrector = corrector;
        }


        /// <summary>
        /// Process the pulse sequence, including slicing, phase correction, and accumulation.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence.</param>
        /// <param name="startIndicesList">The list of start indices for pulses of different sources, for example, gas and reference.</param>
        /// <param name="pulseLength">The length of every pulse</param>
        /// <returns>The processed result of the pulse sequence, or null if failed</returns>
        public List<T> Process(double[] pulseSequence, List<List<int>> startIndicesList, int pulseLength,int crestIndex)
        {
            _corrector.ClearBuffer(); //todo 1st time 
            return
                startIndicesList.Select(
                    startIndices => CorrectSequentially(pulseSequence, startIndices, pulseLength, crestIndex)).ToList();
        }

        private T CorrectSequentially(double[] pulseSequence, List<int> startIndices, int pulseLength, int crestIndex)
        {
            foreach (var startIndex in startIndices)
            {
                // todo: exception
                _corrector.Correct(pulseSequence, startIndex, pulseLength, crestIndex);
            }
            return (T) _corrector.OutputSpetrumBuffer().Clone();
        }
    }
}