using System;

namespace PhaseSonar.Maths.Apodizers {
    public class MathNetApodizer : IApodizer {
        private readonly Func<int, double[]> _windowFunc;
        private double[] _hann;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public MathNetApodizer(Func<int, double[]> windowFunc) {
            _windowFunc = windowFunc;
        }

        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse) {
            if (_hann == null) {
                _hann = _windowFunc(symmetryPulse.Length);
            }
            Functions.MultiplyInPlace(symmetryPulse, _hann);
        }
    }
}