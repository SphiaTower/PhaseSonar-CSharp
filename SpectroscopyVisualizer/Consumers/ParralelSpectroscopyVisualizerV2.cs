using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class ParralelSpectroscopyVisualizerV2 : IConsumerV2 {
        private readonly ParallelConsumerV2<SampleRecord, IPulseSequenceProcessor, ResultImpl> _consumer;


        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        /// <param name="queue"></param>
        /// <param name="workers"></param>
        /// <param name="adapter"></param>
        /// <param name="writer"></param>
        /// <param name="targetCnt"></param>
        public ParralelSpectroscopyVisualizerV2(BlockingCollection<SampleRecord> queue,
            IEnumerable<IPulseSequenceProcessor> workers, DisplayAdapter adapter,
            [CanBeNull] IWriterV2<TracedSpectrum> writer,
            int? targetCnt) {
            _consumer = new ParallelConsumerV2<SampleRecord, IPulseSequenceProcessor, ResultImpl>(
                queue, workers, ProcessElement, HandleResultSync, 5000, targetCnt);
            Adapter = adapter;
            Writer = writer;
            TargetAmountReached += OnTargetAmountReached;
        }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum SumSpectrum { get; protected set; }

        /// <summary>
        ///     A Writer for data storage.
        /// </summary>
        [CanBeNull]
        public IWriterV2<TracedSpectrum> Writer { get; }

        /// <summary>
        ///     <see cref="DisplayAdapter" />
        /// </summary>
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

        private void OnTargetAmountReached() {
            Writer?.Write(new TracedSpectrum(SumSpectrum, "accumulated"));
        }

        private void HandleResultSync([NotNull] ResultImpl result) {
            result.Spectrum.IfPresent(spectrum => {
                if (SumSpectrum == null) {
                    SumSpectrum = spectrum.Clone();
                } else {
                    SumSpectrum.TryAbsorb(spectrum);
                }
                Writer?.Write(new TracedSpectrum(spectrum, result.TAG));
                Adapter.UpdateData(spectrum, SumSpectrum);
            });
        }

        [NotNull]
        private ResultImpl ProcessElement([NotNull] SampleRecord record, [NotNull] IPulseSequenceProcessor processor) {
            var elementSpectrum = processor.Process(record.PulseSequence);
            return new ResultImpl(elementSpectrum, record.Id.ToString());
        }

        private class ResultImpl : IResult {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public ResultImpl(Maybe<ISpectrum> spectrum, string tag) {
                Spectrum = spectrum;
                TAG = tag;
            }

            public Maybe<ISpectrum> Spectrum { get; }
            public string TAG { get; }
            public bool Success => Spectrum.IsPresent();
        }
    }
}