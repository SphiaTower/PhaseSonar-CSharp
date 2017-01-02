using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using PhaseSonar.Analyzers.WithoutReference;
using PhaseSonar.Correctors;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class ParralelSpectroscopyVisualizerV2 : IConsumerV2 {
        private readonly ParallelConsumerV2<SampleRecord, IPulseSequenceProcessor, TaggedProcessResult> _consumer;
        private readonly bool _saveAcc;
        private readonly bool _saveSpec;


        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        /// <param name="queue"></param>
        /// <param name="workers"></param>
        /// <param name="adapter"></param>
        /// <param name="writer"></param>
        /// <param name="targetCnt"></param>
        public ParralelSpectroscopyVisualizerV2(BlockingCollection<SampleRecord> queue,
            IEnumerable<IPulseSequenceProcessor> workers, SpectrumDisplayAdapter adapter,
            [CanBeNull] IWriterV2<TracedSpectrum> writer, int waitEmptyProducerMsTimeOut,
            int? targetCnt, bool saveSpec, bool saveAcc) {
            _saveSpec = saveSpec;
            _saveAcc = saveAcc;
            _consumer = new ParallelConsumerV2<SampleRecord, IPulseSequenceProcessor, TaggedProcessResult>(
                queue, workers, ProcessElement, HandleResultSync, waitEmptyProducerMsTimeOut, targetCnt);
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
        ///     <see cref="SpectrumDisplayAdapter" />
        /// </summary>
        public SpectrumDisplayAdapter Adapter { get; set; }

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt => _consumer.ConsumedCnt;

        public int? TargetCnt => _consumer.TargetCnt;

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            OnTargetAmountReached();
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

        private void OnTargetAmountReached() {
            if (_saveAcc) {
                Writer?.Write(new TracedSpectrum(SumSpectrum, "Accumulated"));
            }
        }

        private void HandleResultSync([NotNull] TaggedProcessResult taggedProcessResult) {
            var spectrum = taggedProcessResult.Spectrum;
            if (taggedProcessResult.IsSuccessful) {
                if (SumSpectrum == null) {
                    SumSpectrum = spectrum.Clone();
                } else {
                    SumSpectrum.TryAbsorb(spectrum);
                }
                if (_saveSpec) {
                    Writer?.Write(new TracedSpectrum(spectrum, taggedProcessResult.Tag));
                }
                Adapter.UpdateData(spectrum, SumSpectrum);
            }
        }

        [NotNull]
        private TaggedProcessResult ProcessElement([NotNull] SampleRecord record,
            [NotNull] IPulseSequenceProcessor processor) {
            var processResult = processor.Process(record.PulseSequence);
            return new TaggedProcessResult(processResult, record.Id);
        }

        private class TaggedProcessResult : IResult {
            private readonly AccumulationResult _accumulationResult;

            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public TaggedProcessResult(AccumulationResult accumulationResult, string tag) {
                _accumulationResult = accumulationResult;
                Tag = tag;
            }

            public ISpectrum Spectrum => _accumulationResult.Data;
            public string Tag { get; }
            public bool IsSuccessful => _accumulationResult.HasSpectrum;
            public bool HasException => _accumulationResult.HasException;
            public ProcessException? Exception => _accumulationResult.Exception;
            public int ExceptionCnt => _accumulationResult.ExceptionCnt;
            public int ValidPeriodCnt => _accumulationResult.HasSpectrum ? _accumulationResult.Data.PulseCount : 0;
        }
    }
}