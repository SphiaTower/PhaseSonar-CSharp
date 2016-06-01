using System.Collections.Generic;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    /// A parallel accumulator
    /// </summary>
    /// <typeparam name="T">The type of the spectrum.</typeparam>
    public sealed class ParallelAccumulator<T> : Accumulator<T> where T : ISpectrum {
        
        /// <summary>
        /// The strategy it uses.
        /// </summary>
        protected override IAnalyzerStrategy<T> Strategy { get; set; }


        /// <summary>
        /// Create a parallel accumulator.
        /// </summary>
        /// <param name="slicer">A slicer</param>
        /// <param name="correctors">Working correctors</param>
        public ParallelAccumulator(ISlicer slicer, List<ICorrector<T>> correctors) : base(slicer)
        {
            Strategy = new ParallelStrategy<T>(correctors);
        }
    }
}