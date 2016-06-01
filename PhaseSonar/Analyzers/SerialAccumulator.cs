using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     A serial accumulator that process pulses one by one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SerialAccumulator<T> : Accumulator<T> where T : ISpectrum
    {
        /// <summary>
        ///     Create a serial accumulator.
        /// </summary>
        /// <param name="slicer">
        ///     <see cref="ISlicer" />
        /// </param>
        /// <param name="corrector">
        ///     <see cref="ICorrector{T}" />
        /// </param>
        public SerialAccumulator(ISlicer slicer, ICorrector<T> corrector) : base(slicer)
        {
            Strategy = new SerialStrategy<T>( corrector);
        }

        /// <summary>
        ///     <see cref="IAnalyzerStrategy{T}" />
        /// </summary>
        protected override IAnalyzerStrategy<T> Strategy { get; set; }
    }
}