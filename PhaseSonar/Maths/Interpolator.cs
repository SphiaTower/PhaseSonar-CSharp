using MathNet.Numerics.Interpolation;

namespace PhaseSonar.Maths {
    /// <summary>
    ///     An interpolator which interpolates data
    /// </summary>
    public class Interpolator {
        private readonly double[] _nexAxis;
        private readonly double[] _oldAxis;

        /// <summary>
        ///     Create an interpolator which interpolates data of the specified size
        /// </summary>
        /// <param name="oldSize">The size of data before interpolation</param>
        /// <param name="newSize">The size of data after interpolation</param>
        public Interpolator(int oldSize, int newSize) {
            _oldAxis = new double[oldSize];
            for (var i = 0; i < oldSize; i++) {
                _oldAxis[i] = i;
            }
            _nexAxis = new double[newSize];
            Functions.LineSpaceInPlace(0, oldSize - 1, newSize, _nexAxis);
        }

        /// <summary>
        ///     Interpolate the data into a larger size
        /// </summary>
        /// <param name="original">The data to be interpolated</param>
        /// <param name="interpolated">The output interpolated data</param>
        public void Interpolate(double[] original, double[] interpolated) {
            var interpolate = Interpolation.CreateLinearSpline(_oldAxis, original);
            for (var i = 0; i < _nexAxis.Length; i++) {
                interpolated[i] = interpolate.Interpolate(_nexAxis[i]);
            }
        }
    }
}