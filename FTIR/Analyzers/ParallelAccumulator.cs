using System.Collections.Generic;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers
{
    public sealed class ParallelAccumulator<T> : Accumulator<T> where T : ISpectrum {
        
        protected override IAnalyzerStrategy<T> Strategy { get; set; }


        public ParallelAccumulator(ISlicer slicer, List<ICorrector<T>> correctors) : base(slicer)
        {
            Strategy = new ParallelStrategy<T>(this, correctors);
        }
    }
}