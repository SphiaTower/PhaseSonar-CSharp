using System;
using System.Linq;
using FTIR.Correctors;
using FTIR.Slicers;

namespace FTIR.Analyzers
{
    public sealed class Splitter<T> : BaseAnalyzer where T:ISpectrum
    {
       
        private IAnalyzerStrategy<T> Strategy { get; }



        public SplitResult<T> Split(double[] pulseSequence)
        {
            var specInfos = Strategy.Run(pulseSequence);
            if (specInfos.Count != 2)
            {
                throw new IndexOutOfRangeException("Split result must contain two SpecInfos");
            }
            return SplitResult<T>.EitherAndOther(specInfos[0], specInfos[1]);
        }

        public Splitter(ISlicer slicer) : base(slicer)
        {//todo
        }
    }

    public class SplitResult<T>
    {
        private SplitResult(T source, T reference)
        {
            Source = source;
            Reference = reference;
        }

        public T Source { get; }
        public T Reference { get; }

        public static SplitResult<T> SourceAndRef(T source, T reference)
        {
            return new SplitResult<T>(source, reference);
        }

        public static SplitResult<T> EitherAndOther(T either, T other)
        {
//            return either.Amplitudes.Sum() >= other.Amplitudes.Sum()
//                ? SourceAndRef(either, other)
//                : SourceAndRef(other, either);
            return null;
            // todo
        }
    }
}