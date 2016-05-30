using System.Collections.Concurrent;
using FTIR.Analyzers;
using FTIR.Correctors;
using JetBrains.Annotations;
using Shokouki.Controllers;
using Shokouki.Presenters;

namespace Shokouki.Consumers
{
    internal class SimpleSpetrumViewer<T> : UiConsumer<double[]> where T : ISpectrum
    {
        private double[] _dummyAxis;

        private int _refreshCnt;

        public SimpleSpetrumViewer(BlockingCollection<double[]> blockingQueue, CanvasView view,
            Accumulator<T> accumulator,
            DisplayAdapter adapter, Camera<T> camera)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
            Camera = camera;
        }

        public Accumulator<T> Accumulator { get; }

        public Camera<T> Camera { get; }

        public override bool Save { get; set; }

        public override bool ConsumeElement([NotNull] double[] item)
        {
            var result = Accumulator.Accumulate(item);
            if (result == null) return false;
            OnDataUpdatedInBackground(result);
            return true;
        }

        protected void OnDataUpdatedInBackground([NotNull] T single)
        {
            var averSingle = Adapter.SampleAverageAndSquare(single);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averSingle);
            View.InvokeAsync(() =>
            {
                var averSinglePts = Adapter.CreateGraphPoints(_dummyAxis, averSingle);
                // todo: why does lines above must be exec in the UI thread??
                View.ClearWaveform();

                View.DrawWaveform(averSinglePts);

                if (Camera.IsOn) Camera.Capture(single);

                _refreshCnt++;
            }
                );
        }
    }
}