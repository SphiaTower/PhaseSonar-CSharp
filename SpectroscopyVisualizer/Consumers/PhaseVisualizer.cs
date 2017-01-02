using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using PhaseSonar.Analyzers.PhaseAnalyzers;
using PhaseSonar.Analyzers.WithoutReference;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class PhaseVisualizer : IConsumerV2 {
        private readonly SerialConsumerV2<SampleRecord> _consumer;
        private readonly IPhaseReader _worker;

        public PhaseVisualizer(BlockingCollection<SampleRecord> queue,
            IPhaseReader worker, PhaseDisplayAdapter adapter, [CanBeNull] IWriterV2<TracedSpectrum> writer,
            int waitEmptyProducerMsTimeout,
            int? targetCnt) {
            _worker = worker;
            _consumer = new SerialConsumerV2<SampleRecord>(ProcessElement, queue, targetCnt, waitEmptyProducerMsTimeout);
            Adapter = adapter;
            Writer = writer;
        }

        public IWriterV2<TracedSpectrum> Writer { get; set; }

        public PhaseDisplayAdapter Adapter { get; set; }

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

        public event UpdateEventHandler Update {
            add { _consumer.Update += value; }
            remove { _consumer.Update -= value; }
        }


        public event Action ProducerEmpty {
            add { _consumer.ProducerEmpty += value; }
            remove { _consumer.ProducerEmpty -= value; }
        }

        public event Action TargetAmountReached {
            add { _consumer.TargetAmountReached += value; }
            remove { _consumer.TargetAmountReached -= value; }
        }

        [NotNull]
        public IResult ProcessElement([NotNull] SampleRecord record) {
            var result = _worker.GetPhase(record.PulseSequence);
            if (result.HasSpectrum) {
                Adapter.UpdatePhase(result.Data);
//                Writer?.Write(new TracedSpectrum(result.Data, record.Id.ToString()));
            }
            return new PhaseResultAdapter(result);
        }

        private class PhaseResultAdapter : IResult {
            private readonly PhaseResult _resultImplementation;

            public PhaseResultAdapter(PhaseResult resultImplementation) {
                _resultImplementation = resultImplementation;
            }

            public bool IsSuccessful => _resultImplementation.HasSpectrum;

            public bool HasException => _resultImplementation.HasException;

            public ProcessException? Exception => _resultImplementation.Exception;

            public int ExceptionCnt => _resultImplementation.ExceptionCnt;
            public int ValidPeriodCnt => _resultImplementation.HasSpectrum ? 1 : 0;
        }
    }
}