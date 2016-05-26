using System.Collections.Generic;
using System.Linq;
using FTIR.Correctors;

namespace FTIR.Analyzers
{
    public class SequentialStrategy<T> : IAnalyzerStrategy<T> where T : ISpectrum
    {
        private readonly ICorrector<T> _corrector;

        public SequentialStrategy(BaseAnalyzer analyzer, ICorrector<T> corrector)
        {
            Analyzer = analyzer;
            _corrector = corrector;
        }

        private BaseAnalyzer Analyzer { get; }


        public List<T> Run(double[] pulseSequence)
        {
            var startIndicesDuo = Analyzer.Slicer.Slice(pulseSequence);
            if (startIndicesDuo==null)
            {
                return null;
            }
            var slicedPeriodLength = Analyzer.Slicer.SlicedPeriodLength;
            _corrector.ClearBuffer(); //todo 1st time 
            return
                startIndicesDuo.Select(
                    startIndices => CorrectSequentially(pulseSequence, startIndices, slicedPeriodLength)).ToList();
        }

        private T CorrectSequentially(double[] pulseSequence, List<int> startIndices, int pulseLength)
        {
            foreach (var startIndex in startIndices)
            {
                // todo: exception
                _corrector.Correct(pulseSequence, startIndex, pulseLength, Analyzer.Slicer.SliceStartOffset);
            }
            return (T) _corrector.OutputSpetrumBuffer().Clone();
        }
    }
}