using JetBrains.Annotations;

namespace PhaseSonar.Maths {
    /// <summary>
    ///     An interpolator which interpolates data
    /// </summary>
    public class Interpolator {
        private readonly double[] _newAxis;
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
            _newAxis = new double[newSize];
            Functions.LineSpaceInPlace(0, oldSize - 1, newSize, _newAxis);
        }

        /// <summary>
        ///     Interpolate the data into a larger size
        /// </summary>
        /// <param name="original">The data to be interpolated</param>
        /// <param name="interpolated">The output interpolated data</param>
        public void Interpolate(double[] original, [NotNull] double[] interpolated) {
            var interpolate = MathNet.Numerics.Interpolate.Linear(_oldAxis, original);
            for (var i = 0; i < interpolated.Length; i++) {
                interpolated[i] = interpolate.Interpolate(_newAxis[i]);
            }
        }
    }
}