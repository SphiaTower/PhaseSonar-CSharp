using System.Collections.Generic;
using PhaseSonar.Correctors;
using JetBrains.Annotations;

namespace PhaseSonar.Analyzers
{
    public interface IAnalyzerStrategy<T> where T:ISpectrum
    {
        [CanBeNull]
        List<T> Run(double[] pulseSequence);
    }
}