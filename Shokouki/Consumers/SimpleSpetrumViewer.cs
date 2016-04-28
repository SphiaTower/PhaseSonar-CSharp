using System.Collections.Concurrent;
using System.Windows.Media;
using FTIR.Analyzers;
using JetBrains.Annotations;
using Shokouki.Controllers;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    internal class SimpleSpetrumViewer : UiConsumer<double[]>
    {
        private double[] _dummyAxis;

        private int _refreshCnt;

        public SimpleSpetrumViewer(BlockingCollection<double[]> blockingQueue, IScopeView view, Accumulator accumulator,
            DisplayAdapter adapter)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
        }

        public Accumulator Accumulator { get; }

        public Camera Camera { get; } = new DiskCamera();

        public override void ConsumeElement([NotNull] double[] item)
        {
            var result = Accumulator.Accumulate(item);
            IsOn = false;
            if (result == null) return;
            OnDataUpdatedInBackground(result);
        }

        protected void OnDataUpdatedInBackground([NotNull] SpecInfo single)
        {
            var averSingle = Adapter.DownSampleAndAverage(single.Spec, single.PeriodCnt);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averSingle);
            View.Invoke(() =>
            {
                var averSinglePts = Adapter.ToPoints(_dummyAxis, averSingle);
                // todo: why does lines above must be exec in the UI thread??
                View.Clear();

                View.DrawLine(averSinglePts);

                if (Camera.IsOn) Camera.Capture(single);

                _refreshCnt++;
            }
                );
        }
    }
}

