using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers {
    public sealed class SequentialAccumulator<T>:Accumulator<T> where T : ISpectrum
    {
      
        protected override IAnalyzerStrategy<T> Strategy { get; set; }

        public SequentialAccumulator(ISlicer slicer,ICorrector<T> corrector) : base(slicer)
        {
            Strategy = new SequentialStrategy<T>(this,corrector);
        }
    }
}
