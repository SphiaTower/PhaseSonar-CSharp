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

/*
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
        }*/
    }
}