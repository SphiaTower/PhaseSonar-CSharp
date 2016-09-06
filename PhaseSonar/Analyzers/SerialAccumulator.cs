using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     A serial accumulator that process pulses one by one.
    /// </summary>
    public sealed class SerialAccumulator : Accumulator {
        /// <summary>
        ///     Create a serial accumulator.
        /// </summary>
        /// <param name="slicer">
        ///     <see cref="ISlicer" />
        /// </param>
        /// <param name="corrector">
        ///     <see cref="ICorrector" />
        /// </param>
        public SerialAccumulator(ISlicer slicer, ICorrector corrector) : base(slicer) {
            Strategy = new SerialStrategy(corrector);
        }

        /// <summary>
        ///     <see cref="IAnalyzerStrategy" />
        /// </summary>
        protected override IAnalyzerStrategy Strategy { get; set; }
    }
}