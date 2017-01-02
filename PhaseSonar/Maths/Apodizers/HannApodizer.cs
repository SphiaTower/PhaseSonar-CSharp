using MathNet.Numerics;

namespace PhaseSonar.Maths.Apodizers {
    public class HannApodizer : MathNetApodizer {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public HannApodizer() : base(Window.Hann) {
        }
    }
}