using MathNet.Numerics;

namespace PhaseSonar.Maths.Apodizers {
    public class HammingApodizer : MathNetApodizer {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public HammingApodizer() : base(Window.Hamming) {
        }
    }
}