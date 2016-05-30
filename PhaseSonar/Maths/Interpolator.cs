using MathNet.Numerics.Interpolation;

namespace PhaseSonar.Maths
{
    public class Interpolator
    {
        private readonly double[] _nexAxis;
        private readonly double[] _oldAxis;

        public Interpolator(int oldLength, int newLength)
        {
            _oldAxis = new double[oldLength];
            for (var i = 0; i < oldLength; i++)
            {
                _oldAxis[i] = i;
            }
            _nexAxis = new double[newLength];
            Funcs.LineSpaceInPlace(0, oldLength - 1, newLength, _nexAxis);
        }

        public void Interpolate(double[] original, double[] interpolated)
        {
            var interpolate = Interpolation.CreateLinearSpline(_oldAxis, original);
            for (var i = 0; i < _nexAxis.Length; i++)
            {
                interpolated[i] = interpolate.Interpolate(_nexAxis[i]);
            }
        }
    }
}