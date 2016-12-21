using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    internal class RefTaggedResult : IResult {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public RefTaggedResult(SplitResult splitResult, string tag) {
            SplitResult = splitResult;
            Tag = tag;
        }
        // todo
        public SplitResult SplitResult { get; }
        public string Tag { get; }
        public bool IsSuccessful => SplitResult.HasSpectrum;
        public bool HasException => SplitResult.HasException;
        public ProcessException? Exception => SplitResult.Exception;
        public int ExceptionCnt => SplitResult.ExceptionCnt;
        public int ValidPeriodCnt => SplitResult.HasSpectrum ? SplitResult.Spectrum.Gas.PulseCount : 0;
    }

    internal class RefSpectroscopyVisualizer : IConsumerV2 {
        private readonly bool _saveSpec;
        private readonly bool _saveAcc;

        private readonly ParallelConsumerV2<SampleRecord, IRefPulseSequenceProcessor, RefTaggedResult>
            _consumerV2Implementation;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public RefSpectroscopyVisualizer(BlockingCollection<SampleRecord> queue,
            IEnumerable<IRefPulseSequenceProcessor> workers, DisplayAdapter adapter,
            [CanBeNull] IWriterV2<TracedSpectrum> writer,
            int? targetCnt,bool saveSpec,bool saveAcc) {
            _saveSpec = saveSpec;
            _saveAcc = saveAcc;
            _consumerV2Implementation = new ParallelConsumerV2<SampleRecord, IRefPulseSequenceProcessor, RefTaggedResult>(
                queue, workers, ProcessElement, HandleResultSync, 5000, targetCnt);
            Adapter = adapter;
            Writer = writer;
            TargetAmountReached += OnTargetAmountReached;
        }

        /// <summary>
        ///     The accumulation of spectra.
        /// </summary>
        [CanBeNull]
        public ISpectrum GasSumSpectrum { get; protected set; }

        public ISpectrum RefSumSpectrum { get; protected set; }

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
        public int ConsumedCnt => _consumerV2Implementation.ConsumedCnt;

        public int? TargetCnt => _consumerV2Implementation.TargetCnt;

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            OnTargetAmountReached();
            _consumerV2Implementation.Stop();
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            _consumerV2Implementation.Start();
        }

        event Action IConsumerV2.SourceInvalid {
            add { _consumerV2Implementation.SourceInvalid += value; }
            remove { _consumerV2Implementation.SourceInvalid -= value; }
        }

        public event UpdateEventHandler Update {
            add { _consumerV2Implementation.Update += value; }
            remove { _consumerV2Implementation.Update -= value; }
        }

        public event Action SourceInvalid {
            add { _consumerV2Implementation.SourceInvalid += value; }
            remove { _consumerV2Implementation.SourceInvalid -= value; }
        }


        public event Action ProducerEmpty {
            add { _consumerV2Implementation.ProducerEmpty += value; }
            remove { _consumerV2Implementation.ProducerEmpty -= value; }
        }

        public event Action TargetAmountReached {
            add { _consumerV2Implementation.TargetAmountReached += value; }
            remove { _consumerV2Implementation.TargetAmountReached -= value; }
        }

        private void OnTargetAmountReached() {
            if (_saveAcc) {

                Task.Run(() => {
                    Writer?.Write(new TracedSpectrum(GasSumSpectrum, "accumulated-GAS"));
                    Writer?.Write(new TracedSpectrum(RefSumSpectrum, "accumulated-REF"));
                    Writer?.Write(new TracedSpectrum(Transmit(GasSumSpectrum, RefSumSpectrum), "accumulated-transmit"));
                });
            }
        }

        private void HandleResultSync(RefTaggedResult refTaggedResult) {
            if (refTaggedResult.IsSuccessful) {
                var split = refTaggedResult.SplitResult.Spectrum;
                if (GasSumSpectrum == null) {
                    GasSumSpectrum = split.Gas.Clone();
                    RefSumSpectrum = split.Reference.Clone();
                } else {
                    GasSumSpectrum.TryAbsorb(split.Gas);
                    RefSumSpectrum.TryAbsorb(split.Reference);
                }
                if (_saveSpec) {

                    Writer?.Write(new TracedSpectrum(split.Gas, refTaggedResult.Tag + "-GAS"));
                    Writer?.Write(new TracedSpectrum(split.Reference, refTaggedResult.Tag + "-REF"));
                }
                //Adapter.UpdateData(Transmit(split.Gas, split.Reference), Transmit(GasSumSpectrum, RefSumSpectrum));
                Adapter.UpdateData(split.Gas, GasSumSpectrum);
            }
        }

        public ISpectrum Transmit(ISpectrum gas, ISpectrum reference) {
            var transmit = new Complex[gas.Length()];
            for (var i = 0; i < gas.Length(); i++) {
                transmit[i] = gas.Array[i]/gas.PulseCount/(reference.Array[i]/reference.PulseCount);
            }
            return new Spectrum(transmit, 1);
        }

        [NotNull]
        private RefTaggedResult ProcessElement([NotNull] SampleRecord record, [NotNull] IRefPulseSequenceProcessor processor) {
            var splitResult = processor.Process(record.PulseSequence);
            return new RefTaggedResult(splitResult, record.Id);
        }
    }
}