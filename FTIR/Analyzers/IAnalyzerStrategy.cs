using System.Collections.Generic;
using JetBrains.Annotations;

namespace FTIR.Analyzers
{
    public interface IAnalyzerStrategy
    {
        [CanBeNull]
        List<SpecInfo> Run(double[] pulseSequence);
    }
}