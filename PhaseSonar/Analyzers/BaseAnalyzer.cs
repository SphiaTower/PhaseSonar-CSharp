using System.Collections.Generic;
using PhaseSonar.Correctors;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers
{
    public abstract class BaseAnalyzer
    {
        public ISlicer Slicer { get; set; }

        protected BaseAnalyzer(ISlicer slicer)
        {
            Slicer = slicer;
        }

    }
}