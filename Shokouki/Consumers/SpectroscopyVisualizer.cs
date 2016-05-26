using System.Collections.Concurrent;
using System.Windows.Media;
using FTIR.Analyzers;
using FTIR.Correctors;
using JetBrains.Annotations;
using Shokouki.Controllers;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    public class SpectroscopyVisualizer<T> : UiConsumer<double[]> where T : ISpectrum
    {
        private double[] _dummyAxis;
        private int _refreshCnt;

        public SpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            IScopeView view,
            Accumulator<T> accumulator,
            DisplayAdapter adapter,
            Camera<T> camera)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
            Camera = camera;
        }

        public Accumulator<T> Accumulator { get; }
        public ISpectrum SumSpectrum { get; protected set; }
        public Camera<T> Camera { get; }

        public override bool Save
        {
            get { return Camera.IsOn; }
            set { Camera.IsOn = value; }
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

        public override void ConsumeElement([NotNull] double[] item)
        {
            var elementSpectrum = Accumulator.Accumulate(item);
            if (elementSpectrum == null)
            {
                return;
            }
            if (SumSpectrum == null)
            {
                SumSpectrum = elementSpectrum.Clone();
            }
            else
            {
                SumSpectrum.TryAbsorb(elementSpectrum);
            }
            OnDataUpdatedInBackground(elementSpectrum);
        }

        protected void OnDataUpdatedInBackground([NotNull] T single)
        {
            var averAll = Adapter.DownSampleAndAverage(SumSpectrum);
            var averSingle = Adapter.DownSampleAndAverage(single);

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
                // todo: bug: buffer cleared while closure continues
                _refreshCnt++;
            }
                );
        }
    }
}