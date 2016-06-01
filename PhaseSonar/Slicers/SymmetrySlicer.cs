using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseSonar.Slicers {
    public class SymmetrySlicer:SimpleSlicer {
       

        public override int CrestIndex => SlicedPeriodLength/2;


        public SymmetrySlicer(ICrestFinder crestFinder) : base(crestFinder)
        {
        }
    }
}
