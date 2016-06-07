using System.Collections.Concurrent;
using System.Windows.Media;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using JetBrains.Annotations;
using SpectroscopyVisualizer.Writers;
using SpectroscopyVisualizer.Presenters;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    /// A consumer which processes data sampled and updates display.
    /// </summary>
    /// <typeparam name="T">The type of the spectrum.</typeparam>
    public class SpectroscopyVisualizer<T> : UiConsumer<double[]> where T : ISpectrum
    {
        private double[] _dummyAxis;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="blockingQueue">The queue containing all elements to be consumed.</param>
        /// <param name="view">The view for display.</param>
        /// <param name="accumulator">An accumulator processing data sampled.</param>
        /// <param name="adapter">An adapter for display.</param>
        /// <param name="writer">A Writer for data storage.</param>
        public SpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            CanvasView view,
            Accumulator<T> accumulator,
            DisplayAdapter adapter,
            Writer<T> writer)
            : base(blockingQueue, view, adapter)
        {
            Accumulator = accumulator;
            Writer = writer;
        }

        /// <summary>
        /// An accumulator processing data sampled.
        /// </summary>
        public Accumulator<T> Accumulator { get; }
        /// <summary>
        /// The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }
        /// <summary>
        /// A Writer for data storage.
        /// </summary>
        public Writer<T> Writer { get; }

        /// <summary>
        /// Save data.
        /// </summary>
        public override bool Save
        {
            get { return Writer.IsOn; }
            set { Writer.IsOn = value; }
        }


        /// <summary>
        /// Start consuming.
        /// </summary>
        public override void Consume()
        {
            SumSpectrum?.Clear();
            base.Consume();
        }

        /// <summary>
        /// Comsume an element dequeued from the queue.
        /// </summary>
        /// <param name="item">The element</param>
        /// <returns>Consumed successfully or not</returns>
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

        /// <summary>
        /// Called when a new element is processed in the background.
        /// </summary>
        /// <param name="single">The processed spetrum of a newly dequeued element.</param>
        protected void OnDataUpdatedInBackground([NotNull] T single)
        {
            var averAll = Adapter.SampleAverageAndSquare(SumSpectrum);
            var averSingle = Adapter.SampleAverageAndSquare(single);

            if (Writer.IsOn) Writer.Enqueue(single);

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
            }
                );
        }
    }
}