using System;
using JetBrains.Annotations;
using MathNet.Numerics;

namespace PhaseSonar.Maths {
    /// <summary>
    ///     An apodizer which applies temporal windows on the signal.
    /// </summary>
    public interface IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        void Apodize([NotNull] double[] symmetryPulse);
    }

    /// <summary>
    ///     An fake apodizer which does nothing in fact.
    /// </summary>
    public class FakeApodizer : IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse) {
        }
    }

    /// <summary>
    ///     An apodizer which applies a triangluar window.
    /// </summary>
    public class TriangulerApodizer : IApodizer {
        /// <summary>
        ///     Apodize the pulse
        /// </summary>
        /// <param name="symmetryPulse">The input pulse which has been symmetrized</param>
        public void Apodize(double[] symmetryPulse) {
            var centerBurst = symmetryPulse.Length/2;
            var rampArray = RampGenerator.Ramp(symmetryPulse.Length, centerBurst);
            Functions.MultiplyInPlace(symmetryPulse, rampArray);
        }
    }

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

    public class HannApodizer : MathNetApodizer {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public HannApodizer() : base(Window.Hann) {
        }
    }

    public class HammingApodizer : MathNetApodizer {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public HammingApodizer() : base(Window.Hamming) {
        }
    }

    public class CosineApodizer : MathNetApodizer {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public CosineApodizer() : base(Window.Cosine) {
        }
    }
}