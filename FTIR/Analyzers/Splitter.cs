using System;
using System.Linq;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers
{
    public sealed class Splitter : BaseAnalyzer
    {
       
        private IAnalyzerStrategy Strategy { get; }



        public SplitResult Split(double[] pulseSequence)
        {
            var specInfos = Strategy.Run(pulseSequence);
            if (specInfos.Count != 2)
            {
                throw new IndexOutOfRangeException("Split result must contain two SpecInfos");
            }
            return SplitResult.EitherAndOther(specInfos[0], specInfos[1]);
        }

        public Splitter(ISlicer slicer) : base(slicer)
        {//todo
        }
    }

    public class SplitResult
    {
        private SplitResult(SpecInfo source, SpecInfo reference)
        {
            Source = source;
            Reference = reference;
        }

        public SpecInfo Source { get; }
        public SpecInfo Reference { get; }

        public static SplitResult SourceAndRef(SpecInfo source, SpecInfo reference)
        {
            return new SplitResult(source, reference);
        }

        public static SplitResult EitherAndOther(SpecInfo either, SpecInfo other)
        {
            return either.Spec.Sum() >= other.Spec.Sum()
                ? SourceAndRef(either, other)
                : SourceAndRef(other, either);
        }
    }
}