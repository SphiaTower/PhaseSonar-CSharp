using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhaseSonar.Maths {
    /// <summary>
    /// An apodizer which applies temporal windows on the signal.
    /// </summary>
    public interface IApodizer
    {
        /// <summary>
        /// Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        void Apodize(double[] symmetryPulse);
    }
    /// <summary>
    /// An fake apodizer which does nothing in fact.
    /// </summary>
    public class FakeApodizer : IApodizer
    {
        /// <summary>
        /// Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse)
        {
            return;
        }
    }
    /// <summary>
    /// An apodizer which applies a triangluar window.
    /// </summary>
    public class TriangulerApodizer : IApodizer
    {
        /// <summary>
        /// Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse)
        {
            var centerBurst = symmetryPulse.Length / 2;
            var rampArray = RampGenerator.Ramp(symmetryPulse.Length, centerBurst);
            Functions.MultiplyInPlace(symmetryPulse, rampArray);
        }

    }
}
