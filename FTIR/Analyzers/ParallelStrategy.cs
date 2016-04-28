using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTIR.Correctors;
using FTIR.Maths;

namespace FTIR.Analyzers {
    public class ParallelStrategy : IAnalyzerStrategy
    {
        private readonly List<ICorrector> _workers;
        public BaseAnalyzer Analyzer { get; }

        public ParallelStrategy(BaseAnalyzer baseAnalyzer,List<ICorrector> correctors )
        {
            Analyzer = baseAnalyzer;
            _workers = correctors;
        }


        public List<SpecInfo> Run(double[] pulseSequence)
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

        private SpecInfo CorrectParallelly(double[] pulseSequence, List<int> startIndices,int pulseLength)
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

            double[] sumArray = null;
            var validPeriodCnt = 0;
            _workers.ForEach(corrector =>
            {
                sumArray = sumArray ?? new double[corrector.Output.Length];
                Funcs.AddTo(sumArray, corrector.Output);
                validPeriodCnt += corrector.OutputPeriodCnt();
            });
            return new SpecInfo(sumArray, validPeriodCnt);
        }
    }
}
