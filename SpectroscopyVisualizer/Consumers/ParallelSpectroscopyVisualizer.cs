using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     A parallel version of <see cref="SerialSpectroscopyVisualizer" /> which dequeues elements parallelly.
    /// </summary>
    public class ParallelSpectroscopyVisualizer : ParallelConsumer<SampleRecord,Accumulator>
    {
        private const string Lock = "lock";
        private double[] _dummyAxis;

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="blockingQueue">The queue containing all elements to be consumed.</param>
        /// <param name="accumulators">A list of accumulators processing the data sampled parallelly.</param>
        /// <param name="adapter">An adapter for display.</param>
        /// <param name="writer">A Writer for data storage.</param>
        public ParallelSpectroscopyVisualizer(
            BlockingCollection<SampleRecord> blockingQueue,
            List<SerialAccumulator> accumulators,
            DisplayAdapter adapter,
            SpectrumWriter writer)
            : base(blockingQueue, accumulators)
        {
            Accumulators = accumulators;
            Adapter = adapter;
            Axis = new AxisBuilder(adapter.WavefromView);
            Writer = writer;
        }

        /// <summary>
        ///     A list of accumulators processing the data sampled parallelly.
        /// </summary>
        public List<SerialAccumulator> Accumulators { get; }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        /// <summary>
        ///     A Writer for data storage.
        /// </summary>
        public SpectrumWriter Writer { get; }


        private AxisBuilder Axis { get; }

        /// <summary>
        ///     <see cref="DisplayAdapter" />
        /// </summary>
        public DisplayAdapter Adapter { get; set; }

        public CanvasView View => Adapter.WavefromView;


        protected override bool ConsumeElement([NotNull] SampleRecord record, Accumulator accumulator)
        {

            var elementSpectrum = accumulator.Accumulate(record.PulseSequence);
            elementSpectrum.IfPresent(spectrum =>
            {
                lock (Lock) {
                    if (SumSpectrum == null) {
                        SumSpectrum = spectrum.Clone();
                    } else {
                        SumSpectrum.TryAbsorb(spectrum);
                    }
                }
                if (Writer.IsOn) Writer.Write(new TracedSpectrum(spectrum, record.ID.ToString()));
                OnDataUpdatedInBackground(spectrum);
            });
            return elementSpectrum.IsPresent();
        }

        /// <summary>
        ///     Called when a new element is processed in the background.
        /// </summary>
        /// <param name="singleSpectrum">The processed spetrum of a newly dequeued element.</param>
        /// <param name="id"></param>
        protected void OnDataUpdatedInBackground([NotNull] ISpectrum singleSpectrum)
        {
            var averAll = Adapter.SampleAverageAndSquare(SumSpectrum);
            var averSingle = Adapter.SampleAverageAndSquare(singleSpectrum);

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

        protected override void OnStop()
        {
            base.OnStop();
            if (Writer.IsOn)
            {
                Writer.Write(new TracedSpectrum(SumSpectrum,"accumulated"));
            }
        }
    }
}