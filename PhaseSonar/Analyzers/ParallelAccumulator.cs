using System.Collections.Generic;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    ///     A parallel accumulator
    /// </summary>
    public sealed class ParallelAccumulator : Accumulator
    {
        /// <summary>
        ///     Create a parallel accumulator.
        /// </summary>
        /// <param name="slicer">A slicer</param>
        /// <param name="correctors">Working correctors</param>
        public ParallelAccumulator(ISlicer slicer, IList<ICorrector> correctors) : base(slicer)
        {
            Strategy = new ParallelStrategy(correctors);
        }

        /// <summary>
        ///     The strategy it uses.
        /// </summary>
        protected override IAnalyzerStrategy Strategy { get; set; }
    }
}