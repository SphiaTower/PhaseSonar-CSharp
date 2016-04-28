using System.Collections.Generic;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers
{
    public sealed class ParallelAccumulator : Accumulator
    {
        
        protected override IAnalyzerStrategy Strategy { get; set; }


        public ParallelAccumulator(ISlicer slicer, List<ICorrector> correctors) : base(slicer)
        {
            Strategy = new ParallelStrategy(this, correctors);
        }
    }
}