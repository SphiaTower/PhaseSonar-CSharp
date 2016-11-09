using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class PhaseVisualizer : IConsumerV2 {
        private readonly SerialConsumerV2<SampleRecord> _consumer;
        private readonly IPulseSequenceProcessor _worker;

        public PhaseVisualizer(BlockingCollection<SampleRecord> queue,
            IPulseSequenceProcessor worker, DisplayAdapter adapter, [CanBeNull] IWriterV2<TracedSpectrum> writer,
            int? targetCnt) {
            _worker = worker;
            _consumer = new SerialConsumerV2<SampleRecord>(ProcessElement, queue, targetCnt, 5000);
            Adapter = adapter;
            Writer = writer;
        }

        public IWriterV2<TracedSpectrum> Writer { get; set; }

        public DisplayAdapter Adapter { get; set; }

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt => _consumer.ConsumedCnt;

        public int? TargetCnt => _consumer.TargetCnt;

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            _consumer.Stop();
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            _consumer.Start();
        }

        public event Action SourceInvalid {
            add { _consumer.SourceInvalid += value; }
            remove { _consumer.SourceInvalid -= value; }
        }

        public event Action ElementConsumedSuccessfully {
            add { _consumer.ElementConsumedSuccessfully += value; }
            remove { _consumer.ElementConsumedSuccessfully -= value; }
        }

        public event Action ProducerEmpty {
            add { _consumer.ProducerEmpty += value; }
            remove { _consumer.ProducerEmpty -= value; }
        }

        public event Action TargetAmountReached {
            add { _consumer.TargetAmountReached += value; }
            remove { _consumer.TargetAmountReached -= value; }
        }

        public bool ProcessElement([NotNull] SampleRecord record) {
            var maybe = _worker.Process(record.PulseSequence);
            maybe.IfPresent(spectrum => {
                Adapter.UpdateData(spectrum);
                Writer?.Write(new TracedSpectrum(spectrum, record.Id.ToString()));
            });
            return maybe.IsPresent();
        }
    }
}