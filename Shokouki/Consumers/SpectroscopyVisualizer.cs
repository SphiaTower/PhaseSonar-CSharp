using System.Collections.Concurrent;
using System.Windows.Controls;
using System.Windows.Media;
using FTIR.Analyzers;
using FTIR.Maths;
using FTIR.Utils;
using JetBrains.Annotations;
using Shokouki.Controllers;
using Shokouki.Model;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class SpectroscopyVisualizer : UiConsumer<double[]>
    {
        public Accumulator Accumulator { get; }
        public double[] Accumulated { get; protected set; }
        public int PulseCnt { get; protected set; }

        public Camera Camera { get; } = new DiskCamera();
        private double[] _dummyAxis;

        private int _refreshCnt;

        public SpectroscopyVisualizer(BlockingCollection<double[]> blockingQueue, IScopeView view, Accumulator accumulator, 
            DisplayAdapter adapter)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
        }

        public override void Reset()
        {
            base.Reset();
            Funcs.Clear(Accumulated);
            PulseCnt = 0;
        }

        public override void Consume()
        {
            if (Accumulated != null)
            {
                Funcs.Clear(Accumulated);
            }
            base.Consume();
        }

        public override void ConsumeElement([NotNull]double[] item)
        {
            var result = Accumulator.Accumulate(item);
            if (result == null)
            {
                return;
            }
            Accumulated = Accumulated ?? new double[result.Spec.Length];
            Funcs.AddTo(Accumulated, result.Spec);
            PulseCnt += result.PeriodCnt;

            OnDataUpdatedInBackground(result);
        }

        protected void OnDataUpdatedInBackground([NotNull]SpecInfo single)
        {
            var averAll = Adapter.DownSampleAndAverage(Accumulated, PulseCnt);
            var averSingle = Adapter.DownSampleAndAverage(single.Spec, single.PeriodCnt);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averAll);
            View.Invoke(() =>
            {
                var averSinglePts = Adapter.ToPoints(_dummyAxis, averSingle);
                var averAllPts = Adapter.ToPoints(_dummyAxis, averAll);
                // todo: why does lines above must be exec in the UI thread??
                View.Clear();
                View.DrawLine(averSinglePts, Colors.Red);
                View.DrawLine(averAllPts);

                if (Camera.IsOn) Camera.Capture(single);

                _refreshCnt++;
            }
                );
        }
    }
}