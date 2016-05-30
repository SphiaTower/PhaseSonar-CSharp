using System;
using System.Diagnostics.Contracts;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;
using JetBrains.Annotations;
namespace PhaseSonar.Analyzers
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