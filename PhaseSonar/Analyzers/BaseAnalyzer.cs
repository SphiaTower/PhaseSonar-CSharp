using System.Collections.Generic;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    /// <summary>
    /// An basic analyser which slices and processes the pulse sequence.
    /// </summary>
    public abstract class BaseAnalyzer
    {
        /// <summary>
        /// <see cref="ISlicer"/>
        /// </summary>
        public ISlicer Slicer { get; set; }

        /// <summary>
        /// Create an analyzer
        /// </summary>
        /// <param name="slicer"><see cref="ISlicer"/></param>
        protected BaseAnalyzer(ISlicer slicer)
        {
            Slicer = slicer;
        }

    }
}