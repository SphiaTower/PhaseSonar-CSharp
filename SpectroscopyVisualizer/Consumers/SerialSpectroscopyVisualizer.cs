using System.Collections.Concurrent;
using System.Windows.Media;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     A consumer which processes data sampled and updates display.
    /// </summary>
    public class SerialSpectroscopyVisualizer : AbstractConsumer<double[]>
    {
        private double[] _dummyAxis;

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="blockingQueue">The queue containing all elements to be consumed.</param>
        /// <param name="accumulator">An accumulator processing data sampled.</param>
        /// <param name="adapter">An adapter for display.</param>
        /// <param name="writer">A Writer for data storage.</param>
        public SerialSpectroscopyVisualizer(
            BlockingCollection<double[]> blockingQueue,
            Accumulator accumulator,
            DisplayAdapter adapter,
            AbstractWriter<ISpectrum> writer)
            : base(blockingQueue)
        {
            Accumulator = accumulator;
            Writer = writer;
            Adapter = adapter;
            Axis = new AxisBuilder(adapter.WavefromView);
        }

        /// <summary>
        ///     An accumulator processing data sampled.
        /// </summary>
        public Accumulator Accumulator { get; }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        /// <summary>
        ///     A Writer for data storage.
        /// </summary>
        public AbstractWriter<ISpectrum> Writer { get; }

        public CanvasView View => Adapter.WavefromView;

        public AxisBuilder Axis { get; set; }

        /// <summary>
        ///     <see cref="DisplayAdapter" />
        /// </summary>
        public DisplayAdapter Adapter { get; set; }


        /// <summary>
        ///     Start consuming.
        /// </summary>
        public override void Consume()
        {
            SumSpectrum?.Clear();
            base.Consume();
        }

        /// <summary>
        ///     Comsume an element dequeued from the queue.
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
        ///     Called when a new element is processed in the background.
        /// </summary>
        /// <param name="single">The processed spetrum of a newly dequeued element.</param>
        protected void OnDataUpdatedInBackground([NotNull] ISpectrum single)
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