using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers {
    public sealed class SequentialAccumulator:Accumulator {
      
        protected override IAnalyzerStrategy Strategy { get; set; }

        public SequentialAccumulator(ISlicer slicer,ICorrector corrector) : base(slicer)
        {
            Strategy = new SequentialStrategy(this,corrector);
        }
    }
}
