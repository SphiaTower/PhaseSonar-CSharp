using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using JetBrains.Annotations;

namespace PhaseSonar.Analyzers {
    public class ParallelStrategy<T> : IAnalyzerStrategy<T> where T : ISpectrum {
        private readonly List<ICorrector<T>> _workers;
        public BaseAnalyzer Analyzer { get; }

        public ParallelStrategy(BaseAnalyzer baseAnalyzer,List<ICorrector<T>> correctors )
        {
            Analyzer = baseAnalyzer;
            _workers = correctors;
        }

        public List<T> Run(double[] pulseSequence)
        {
            var startIndicesDuo = Analyzer.Slicer.Slice(pulseSequence);
            if (startIndicesDuo==null)
            {
                return null;
            }
            var slicedPeriodLength = Analyzer.Slicer.SlicedPeriodLength;
            _workers.ForEach(corrector => corrector.ClearBuffer());
            return startIndicesDuo.Select(startIndices => CorrectParallelly(pulseSequence, startIndices, slicedPeriodLength)).ToList();
        }

        private T CorrectParallelly(double[] pulseSequence, List<int> startIndices,int pulseLength)
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
                        corrector.Correct(pulseSequence, startIndex, pulseLength, Analyzer.Slicer.SliceStartOffset);
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
