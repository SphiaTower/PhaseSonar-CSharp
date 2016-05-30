using System.Collections.Concurrent;
using System.Windows.Media;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using JetBrains.Annotations;
using SpectroscopyVisualizer.Controllers;
using SpectroscopyVisualizer.Presenters;

namespace SpectroscopyVisualizer.Consumers
{
    public class SpectroscopyVisualizer<T> : UiConsumer<double[]> where T : ISpectrum
    {
        private double[] _dummyAxis;
        private int _refreshCnt;

        public SpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            CanvasView view,
            Accumulator<T> accumulator,
            DisplayAdapter adapter,
            Camera<T> camera)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
            Camera = camera;
        }

        public Accumulator<T> Accumulator { get; }
        [CanBeNull]
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
            // todo potential bug
        }

        public override void Consume()
        {
            SumSpectrum?.Clear();
            base.Consume();
        }

        public override bool ConsumeElement([NotNull] double[] item)
        {
            var elementSpectrum = Accumulator.Accumulate(item);
            if (elementSpectrum == null)
            {
                return false;
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
            return true;
        }

        protected void OnDataUpdatedInBackground([NotNull] T single)
        {
            var averAll = Adapter.SampleAverageAndSquare(SumSpectrum);
            var averSingle = Adapter.SampleAverageAndSquare(single);

            if (Camera.IsOn) Camera.Capture(single);

            _dummyAxis = _dummyAxis ?? Axis.DummyAxis(averAll);
            View.InvokeAsync(() =>
            {
                var averSinglePts = Adapter.CreateGraphPoints(_dummyAxis, averSingle);
                var averAllPts = Adapter.CreateGraphPoints(_dummyAxis, averAll);
                // todo: why does lines above must be exec in the UI thread??
                View.ClearWaveform();
//                View.Canvas.Children.RemoveRange(0,2);
                View.DrawWaveform(averSinglePts, Colors.Red);
                View.DrawWaveform(averAllPts);
                // todo: bug: buffer cleared while closure continues
                _refreshCnt++;
            }
                );
        }
    }
}