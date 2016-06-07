using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseSonar.Slicers {
    /// <summary>
    /// A slicer which slices adjacent pulses at the center of them.
    /// </summary>
    public class SymmetrySlicer:SimpleSlicer {
        /// <summary>
        /// The index of the crest in the slice.
        /// </summary>
        public override int CrestIndex => SlicedPeriodLength/2;


        /// <summary>
        /// Create a slicer.
        /// </summary>
        /// <param name="crestFinder"><see cref="ICrestFinder"/></param>
        public SymmetrySlicer(ICrestFinder crestFinder) : base(crestFinder)
        {
        }
    }
}
