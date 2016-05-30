using System.Collections.Generic;
using System.Linq;

namespace SpectroscopyVisualizer.Presenters
{
    public class AxisBuilder
    {
        public AxisBuilder(CanvasView view)
        {
            View = view;
        }

        public CanvasView View { get; set; }

        public double ScreenWidth => View.ScopeWidth;

        private static double[] DummyAxis(double[] dummySource, double scale)
        {
            var axis = new double[dummySource.Length];
            for (var i = 0; i < dummySource.Length; i++)
            {
                axis[i] = i*scale;
            }
            return axis;
        }

        public double[] DummyAxis(double[] dummySource)
        {
            return DummyAxis(dummySource, ScreenWidth/dummySource.Length);
        }

        public double[] ScaleAxis(IEnumerable<int> axis, int maxRange)
        {
            var scaled = new double[axis.Count()];
            var scale = ScreenWidth/maxRange;
            for (var i = 0; i < scaled.Length; i++)
            {
                scaled[i] *= scale;
            }
            return scaled;
        }
    }
}