using System;
using System.Diagnostics.Contracts;
using FTIR.Correctors;
using FTIR.Slicers;
using JetBrains.Annotations;
namespace FTIR.Analyzers
{
    public abstract class Accumulator<T>: BaseAnalyzer where T : ISpectrum
        {
        protected abstract IAnalyzerStrategy<T> Strategy { get; set; }

        [CanBeNull]
        public T Accumulate(double[] pulseSequence)
        {
            var specInfos = Strategy.Run(pulseSequence);
            return specInfos != null ? specInfos[0] : default(T);
        }


        protected Accumulator(ISlicer slicer) : base(slicer)
        {
        }
    }
}