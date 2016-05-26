using System.Collections.Generic;
using FTIR.Correctors;
using JetBrains.Annotations;

namespace FTIR.Analyzers
{
    public interface IAnalyzerStrategy<T> where T:ISpectrum
    {
        [CanBeNull]
        List<T> Run(double[] pulseSequence);
    }
}