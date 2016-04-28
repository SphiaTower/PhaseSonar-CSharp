using System.Collections.Concurrent;
using System.Windows.Controls;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class Oscilloscope : UiConsumer<double[]>
    {
        private double[] _axis;


        public override void ConsumeElement(double[] item)
        {
            var downSampled = Adapter.DownSample(item);
            _axis = _axis ?? Axis.DummyAxis(downSampled);
            View.Invoke(() =>
            {
                var pointCollection = Adapter.ToPoints(_axis, downSampled);
                View.Clear();
                View.DrawLine(pointCollection);
            });
        }

        public Oscilloscope(BlockingCollection<double[]> blockingQueue, IScopeView view, DisplayAdapter adapter) : base(blockingQueue, view, adapter)
        {
        }
    }
}