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
    public sealed class SerialSplitter : SingleDataRecordProcessor
    {
        /// <summary>
        ///     Create a splitter.
        /// </summary>
        /// <param name="slicer">
        ///     <see cref="ISlicer" />
        /// </param>
        /// <param name="corrector">
        ///     <see cref="ICorrector" />
        /// </param>
        public SerialSplitter(ISlicer slicer, ICorrector corrector) : base(slicer)
        {
//todo
            Strategy = new SerialStrategy(corrector);
        }

        private IAnalyzerStrategy Strategy { get; }


        /// <summary>
        ///     Process a pulse sequence and output results of gas and ref
        /// </summary>
        /// <param name="pulseSequence"></param>
        /// <returns>The splitted result</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the data does not contain two components</exception>
        [CanBeNull]
        public SplitResult Split(double[] pulseSequence)
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
            return SplitResult.EitherAndOther(specInfos[0], specInfos[1]);
        }
    }

    /// <summary>
    ///     A container for gas and reference spectra
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SplitResult
    {
        private SplitResult(ISpectrum gas, ISpectrum reference)
        {
            Gas = gas;
            Reference = reference;
        }

        /// <summary>
        ///     The spectrum of the gas
        /// </summary>
        public ISpectrum Gas { get; }

        /// <summary>
        ///     The spectrum of the reference
        /// </summary>
        public ISpectrum Reference { get; }

        /// <summary>
        ///     Create an instance with source and reference known
        /// </summary>
        /// <param name="source">The source spectrum</param>
        /// <param name="reference">The reference spectrum</param>
        /// <returns></returns>
        public static SplitResult SourceAndRef(ISpectrum source, ISpectrum reference)
        {
            return new SplitResult(source, reference);
        }

        /// <summary>
        ///     Create an result instace without the knowledge of which component is gas and which is reference
        /// </summary>
        /// <param name="either">A spectrum</param>
        /// <param name="other">Another spectrum</param>
        /// <returns></returns>
        public static SplitResult EitherAndOther(ISpectrum either, ISpectrum other)
        {
//            return either.Amplitudes.Sum() >= other.Amplitudes.Sum()
//                ? SourceAndRef(either, other)
//                : SourceAndRef(other, either);
            return null;
            // todo
        }
    }
}