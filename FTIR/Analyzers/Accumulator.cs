using System;
using System.Diagnostics.Contracts;
using FTIR.Correctors;
using FTIR.Slicers;
using JetBrains.Annotations;
namespace FTIR.Analyzers
{
    public abstract class Accumulator : BaseAnalyzer
    {
        protected abstract IAnalyzerStrategy Strategy { get; set; }

        [CanBeNull]
        public SpecInfo Accumulate(double[] pulseSequence)
        {
            var specInfos = Strategy.Run(pulseSequence);
            return specInfos?[0];
        }


        protected Accumulator(ISlicer slicer) : base(slicer)
        {
        }
    }
}