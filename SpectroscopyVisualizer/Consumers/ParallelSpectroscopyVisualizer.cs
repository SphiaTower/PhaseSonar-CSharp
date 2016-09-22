using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Media;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    /// <summary>
    ///     A parallel SpectroscopyVisualizer which dequeues elements parallelly, processes them and sends result to display
    ///     layer.
    /// </summary>
    public class ParallelSpectroscopyVisualizer : ParallelConsumer<SampleRecord, IPulseSequenceProcessor> {
        private const string Lock = "lock";

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="blockingQueue">The queue containing all elements to be consumed.</param>
        /// <param name="accumulators">A list of accumulators processing the data sampled parallelly.</param>
        /// <param name="adapter">An adapter for display.</param>
        /// <param name="writer">A Writer for data storage.</param>
        public ParallelSpectroscopyVisualizer(
            [NotNull] BlockingCollection<SampleRecord> blockingQueue,
            [NotNull] List<IPulseSequenceProcessor> accumulators,
            [NotNull] DisplayAdapter adapter,
            SpectrumWriter writer)
            : base(blockingQueue, accumulators) {
            Accumulators = accumulators;
            Adapter = adapter;
            Writer = writer;
            NoProductEvent += OnNoProductEvent;
        }

        /// <summary>
        ///     A list of accumulators processing the data sampled parallelly.
        /// </summary>
        public List<IPulseSequenceProcessor> Accumulators { get; }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        /// <summary>
        ///     A Writer for data storage.
        /// </summary>
        public SpectrumWriter Writer { get; }



        /// <summary>
        ///     <see cref="DisplayAdapter" />
        /// </summary>
        public DisplayAdapter Adapter { get; set; }

        private void OnNoProductEvent(object sender) {
            WriteAccumulated();
        }

        private void WriteAccumulated() {
            if (Writer.IsOn) {
                Writer.Write(new TracedSpectrum(SumSpectrum, "accumulated"));
            }
        }


        /// <summary>
        ///     Consume element in a branch.
        /// </summary>
        /// <param name="element">The product</param>
        /// <param name="worker">The worker in this branch.</param>
        /// <returns>Whether consuming succeeds.</returns>
        protected override bool ConsumeElement([NotNull] SampleRecord element, [NotNull] IPulseSequenceProcessor worker) {
            var elementSpectrum = worker.Accumulate(element.PulseSequence);
            elementSpectrum.IfPresent(spectrum => {
                lock (Lock) {
                    if (SumSpectrum == null) {
                        SumSpectrum = spectrum.Clone();
                    } else {
                        SumSpectrum.TryAbsorb(spectrum);
                    }
                }
                if (Writer.IsOn) Writer.Write(new TracedSpectrum(spectrum, element.Id.ToString()));
                OnDataUpdatedInBackground(spectrum);
            });
            return elementSpectrum.IsPresent();
        }

        /// <summary>
        ///     Called when a new element is processed in the background.
        /// </summary>
        /// <param name="singleSpectrum">The processed spetrum of a newly dequeued element.</param>
        protected void OnDataUpdatedInBackground([NotNull] ISpectrum singleSpectrum) {
            Adapter.UpdateData(singleSpectrum, SumSpectrum);
        }

        /// <summary>
        ///     Called when the consumer is stopped.
        /// </summary>
        protected override void OnStop() {
            WriteAccumulated();
        }
    }
}