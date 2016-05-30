using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseSonar.Maths {
    public interface IApodizer
    {
        void Apodize(double[] symmetryPulse);
    }

    public class FakeApodizer : IApodizer
    {
        public void Apodize(double[] symmetryPulse)
        {
            return;
        }
    }

    public class TriangulerApodizer : IApodizer
    {
        public void Apodize(double[] symmetryPulse)
        {
            var centerBurst = symmetryPulse.Length / 2;
            var rampArray = RampGenerator.Ramp(symmetryPulse.Length, centerBurst);
            Funcs.MultiplyInPlace(symmetryPulse, rampArray);
        }

    }
}
