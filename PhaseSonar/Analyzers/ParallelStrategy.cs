using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using JetBrains.Annotations;

namespace PhaseSonar.Analyzers {
    /// <summary>
    /// A strategy to analyze a pulse sequence. The processing of the pulses are executed in parallel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParallelStrategy<T> : IAnalyzerStrategy<T> where T : ISpectrum {
        private readonly List<ICorrector<T>> _workers;

        /// <summary>
        /// Create a parallel strategy.
        /// </summary>
        /// <param name="correctors">The workers for correcting pulses</param>
        public ParallelStrategy(List<ICorrector<T>> correctors )
        {
            _workers = correctors;
        }

        /// <summary>
        /// Process the pulse sequence, including slicing, phase correction, and accumulation.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence.</param>
        /// <param name="startIndicesList">The list of start indices for pulses of different sources, for example, gas and reference.</param>
        /// <param name="pulseLength">The length of every pulse</param>
        /// <param name="crestIndex"></param>
        /// <returns>The processed result of the pulse sequence, or null if failed</returns>
        public List<T> Process(double[] pulseSequence,[NotNull]List<List<int>> startIndicesList,int pulseLength,int crestIndex)
        {
          
            _workers.ForEach(corrector => corrector.ClearBuffer());
            return startIndicesList.Select(startIndices => CorrectParallelly(pulseSequence, startIndices, pulseLength, crestIndex)).ToList();
        }

        private T CorrectParallelly(double[] pulseSequence, List<int> startIndices,int pulseLength,int crestIndex)
        {
            var queue = new ConcurrentQueue<int>();
            startIndices.ForEach(item => queue.Enqueue(item));
        
            Parallel.ForEach(
                _workers,
                corrector =>
                {
                    int startIndex;
                    while (queue.TryDequeue(out startIndex))
                    {
                        // todo: exception
                        corrector.Correct(pulseSequence, startIndex, pulseLength, crestIndex);
                    }
                }
                );
            var sumSpectrum = (T)_workers[0].OutputSpetrumBuffer().Clone();
            for (int index = 1; index < _workers.Count; index++)
            {
                var corrector = _workers[index];
                sumSpectrum.TryAbsorb(corrector.OutputSpetrumBuffer());
            } // todo all 0 pulse cnt, return null
            return sumSpectrum;
        }
    }
}
