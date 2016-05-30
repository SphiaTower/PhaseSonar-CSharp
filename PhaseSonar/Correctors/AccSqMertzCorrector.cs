using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors {
    public class AccSqMertzCorrector:AccMertzCorrector {
      

        protected override void WriteBuffer(int i, double specPoint)
        {
            base.WriteBuffer(i, specPoint*specPoint);
        }

        public AccSqMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan = 256) : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
        }
    }
}
