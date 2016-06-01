using System;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     A SerialSplitter which splits gas and reference and accumulates results in a pulse sequence respectively.
    ///     TODO: This class may have bugs.
    /// </summary>
    /// <typeparam name="T">The type of the spectrum</typeparam>
    public sealed class SerialSplitter<T> : BaseAnalyzer where T : ISpectrum
    {
        /// <summary>
        ///     Create a splitter.
        /// </summary>
        /// <param name="slicer">
        ///     <see cref="ISlicer" />
        /// </param>
        /// <param name="corrector">
        ///     <see cref="ICorrector{T}" />
        /// </param>
        public SerialSplitter(ISlicer slicer, ICorrector<T> corrector) : base(slicer)
        {
//todo
            Strategy = new SerialStrategy<T>(corrector);
        }

        private IAnalyzerStrategy<T> Strategy { get; }


        /// <summary>
        ///     Process a pulse sequence and output results of gas and ref
        /// </summary>
        /// <param name="pulseSequence"></param>
        /// <returns>The splitted result</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the data does not contain two components</exception>
        [CanBeNull]
        public SplitResult<T> Split(double[] pulseSequence)
        {
            var list = Slicer.Slice(pulseSequence);
            if (list == null)
            {
                return null;
            }
            var specInfos = Strategy.Process(pulseSequence, list, Slicer.SlicedPeriodLength, Slicer.CrestIndex);
            if (specInfos == null)
            {
                return null;
            }
            if (specInfos.Count != 2)
            {
                throw new IndexOutOfRangeException("Split result must contain two SpecInfos");
            }
            return SplitResult<T>.EitherAndOther(specInfos[0], specInfos[1]);
        }
    }

    /// <summary>
    ///     A container for gas and reference spectra
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SplitResult<T>
    {
        private SplitResult(T gas, T reference)
        {
            Gas = gas;
            Reference = reference;
        }

        /// <summary>
        ///     The spectrum of the gas
        /// </summary>
        public T Gas { get; }

        /// <summary>
        ///     The spectrum of the reference
        /// </summary>
        public T Reference { get; }

        /// <summary>
        ///     Create an instance with source and reference known
        /// </summary>
        /// <param name="source">The source spectrum</param>
        /// <param name="reference">The reference spectrum</param>
        /// <returns></returns>
        public static SplitResult<T> SourceAndRef(T source, T reference)
        {
            return new SplitResult<T>(source, reference);
        }

        /// <summary>
        ///     Create an result instace without the knowledge of which component is gas and which is reference
        /// </summary>
        /// <param name="either">A spectrum</param>
        /// <param name="other">Another spectrum</param>
        /// <returns></returns>
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