using System.Collections.Generic;
using System.Linq;
using FTIR.Correctors;

namespace FTIR.Analyzers
{
    public class SequentialStrategy : IAnalyzerStrategy
    {
        private readonly ICorrector _corrector;

        public SequentialStrategy(BaseAnalyzer analyzer, ICorrector corrector)
        {
            Analyzer = analyzer;
            _corrector = corrector;
        }

        private BaseAnalyzer Analyzer { get; }


        public List<SpecInfo> Run(double[] pulseSequence)
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

        private SpecInfo CorrectSequentially(double[] pulseSequence, List<int> startIndices, int pulseLength)
        {
            foreach (var startIndex in startIndices)
            {
                // todo: exception
                _corrector.Correct(pulseSequence, startIndex, pulseLength, Analyzer.Slicer.SliceStartOffset);
            }
            var output = new double[_corrector.Output.Length];
            _corrector.Output.CopyTo(output, 0);
            return new SpecInfo(output, _corrector.OutputPeriodCnt());
        }
    }
}