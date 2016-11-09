using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
     class ResultImpl2 : IResult {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ResultImpl2(Maybe<SplitResult> splitResult, string tag) {
            SplitResult = splitResult;
            TAG = tag;
        }

        public Maybe<SplitResult> SplitResult { get; }
        public string TAG { get; }
        public bool Success => SplitResult.IsPresent();
    }
    class RefSpectroscopyVisualizer: IConsumerV2 {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public RefSpectroscopyVisualizer(BlockingCollection<SampleRecord> queue,
            IEnumerable<IRefPulseSequenceProcessor> workers, DisplayAdapter adapter,
            [CanBeNull] IWriterV2<TracedSpectrum> writer,
            int? targetCnt) {

            _consumerV2Implementation = new ParallelConsumerV2<SampleRecord, IRefPulseSequenceProcessor, ResultImpl2>(
     queue, workers, ProcessElement, HandleResultSync, 5000, targetCnt);
            Adapter = adapter;
            Writer = writer;
            TargetAmountReached += OnTargetAmountReached;
        }

        private void OnTargetAmountReached() {
            Task.Run(() => {

                Writer?.Write(new TracedSpectrum(GasSumSpectrum, "accumulated-GAS"));
                Writer?.Write(new TracedSpectrum(RefSumSpectrum, "accumulated-REF"));
                Writer?.Write(new TracedSpectrum(Transmit(GasSumSpectrum, RefSumSpectrum), "accumulated-transmit"));

            });
        }

        private void HandleResultSync(ResultImpl2 result) {
            result.SplitResult.IfPresent(split => {
                if (GasSumSpectrum == null) {
                    GasSumSpectrum = split.Gas.Clone();
                    RefSumSpectrum = split.Reference.Clone();
                } else {
                    GasSumSpectrum.TryAbsorb(split.Gas);
                    RefSumSpectrum.TryAbsorb(split.Reference);
                }
                Writer?.Write(new TracedSpectrum(split.Gas, result.TAG+"-GAS"));
                Writer?.Write(new TracedSpectrum(split.Reference, result.TAG+"-REF"));
                Adapter.UpdateData(Transmit(split.Gas,split.Reference), Transmit(GasSumSpectrum,RefSumSpectrum));
            });
        }

        public ISpectrum Transmit(ISpectrum gas, ISpectrum reference) {
            Complex[] transmit = new Complex[gas.Length()];
            for (int i = 0; i < gas.Length(); i++) {
                transmit[i] = gas.Array[i]/gas.PulseCount/(reference.Array[i]/reference.PulseCount);
            }
            return new Spectrum(transmit,1);
        }
        [NotNull]
        private ResultImpl2 ProcessElement([NotNull] SampleRecord record, [NotNull] IRefPulseSequenceProcessor processor) {
            var splitResult = processor.Process(record.PulseSequence);
            return new ResultImpl2(splitResult, record.Id.ToString());
        }

        private ParallelConsumerV2<SampleRecord, IRefPulseSequenceProcessor, ResultImpl2> _consumerV2Implementation;
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
            _consumerV2Implementation.Stop();
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            _consumerV2Implementation.Start();
        }

        public event Action SourceInvalid {
            add { _consumerV2Implementation.SourceInvalid += value; }
            remove { _consumerV2Implementation.SourceInvalid -= value; }
        }

        public event Action ElementConsumedSuccessfully {
            add { _consumerV2Implementation.ElementConsumedSuccessfully += value; }
            remove { _consumerV2Implementation.ElementConsumedSuccessfully -= value; }
        }

        public event Action ProducerEmpty {
            add { _consumerV2Implementation.ProducerEmpty += value; }
            remove { _consumerV2Implementation.ProducerEmpty -= value; }
        }

        public event Action TargetAmountReached {
            add { _consumerV2Implementation.TargetAmountReached += value; }
            remove { _consumerV2Implementation.TargetAmountReached -= value; }
        }
    }
}
