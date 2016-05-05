using System.Collections.Concurrent;
using System.Windows.Controls;
using System.Windows.Media;
using FTIR.Analyzers;
using FTIR.Correctors;
using FTIR.Maths;
using FTIR.Utils;
using JetBrains.Annotations;
using Shokouki.Controllers;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class SpectroscopyVisualizer : UiConsumer<double[]>
    {
        public Accumulator Accumulator { get; }
        public ISpectrum SumSpectrum { get; protected set; }
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
            SumSpectrum.Clear();
        }

        public override void Consume()
        {
            SumSpectrum?.Clear();
            base.Consume();
        }

        public override void ConsumeElement([NotNull]double[] item)
        {
            var elementSpectrum = Accumulator.Accumulate(item);
            if (elementSpectrum == null)
            {
                return;
            }
            if (SumSpectrum==null)
            {
                SumSpectrum = elementSpectrum.Clone();
            }
            else
            {
                SumSpectrum.Absorb(elementSpectrum);
            }
            OnDataUpdatedInBackground(elementSpectrum);
        }

        protected void OnDataUpdatedInBackground([NotNull]ISpectrum single)
        {
            var averAll = Adapter.DownSampleAndAverage(SumSpectrum.Amplitudes, SumSpectrum.PulseCount);
            var averSingle = Adapter.DownSampleAndAverage(single.Amplitudes, single.PulseCount);

            if (Camera.IsOn) Camera.Capture(single);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averAll);
            View.InvokeAsync(() =>
            {
                var averSinglePts = Adapter.ToPoints(_dummyAxis, averSingle);
                var averAllPts = Adapter.ToPoints(_dummyAxis, averAll);
                // todo: why does lines above must be exec in the UI thread??
                View.Clear();
                View.DrawLine(averSinglePts, Colors.Red);
                View.DrawLine(averAllPts);


                _refreshCnt++;
            }
                );
        }
    }
}