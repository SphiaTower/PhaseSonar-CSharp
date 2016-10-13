using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer.Consumers {
    public class PulseByPulseChecker : IConsumerV2 {
        private readonly ParallelConsumerV2<SampleRecord, PulseChecker, ProcessResult> _consumer;

        private readonly List<SpecInfoWrapper> _specInfos = new List<SpecInfoWrapper>();

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        /// <param name="queue"></param>
        /// <param name="workers"></param>
        /// <param name="targetCnt"></param>
        public PulseByPulseChecker(BlockingCollection<SampleRecord> queue, IEnumerable<PulseChecker> workers,
            int? targetCnt) {
            _consumer = new ParallelConsumerV2<SampleRecord, PulseChecker, ProcessResult>(
                queue, workers, ProcessFunc, ResultHandleFunc, 2000, targetCnt);
            TargetAmountReached += OnTargetAmountReached;
        }

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
            _specInfos.Sort((lhs, rhs) => {
                if (lhs.FileId > rhs.FileId) {
                    return 1;
                }
                if (lhs.FileId < rhs.FileId) {
                    return -1;
                }
                if (lhs.Number > rhs.Number) {
                    return 1;
                }
                if (lhs.Number < rhs.Number) {
                    return -1;
                }
                return 0;
            });
            var strings = _specInfos.Select(info => info.FileId + " " + info.Number + " " + info.First).ToArray();
            Toolbox.WriteStringArray(@"D:\zbf\temp\pp.txt", strings);
            var aggregate = strings.Aggregate((s1, s2) => s1 + s2);
            Application.Current.MainWindow.Dispatcher.InvokeAsync(() => { Console.Write(aggregate); });
        }

        private void ResultHandleFunc(ProcessResult result) {
            _specInfos.AddRange(result.SpecInfos);
        }

        [NotNull]
        private ProcessResult ProcessFunc(SampleRecord record, PulseChecker checker) {
            var specInfos = checker.Process(record.PulseSequence);
            return new ProcessResult(specInfos.Select(info => new SpecInfoWrapper(info, record.Id)).ToList());
        }

        private class ProcessResult : IResult {
            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            public ProcessResult(IList<SpecInfoWrapper> specInfos) {
                SpecInfos = specInfos;
            }

            public IList<SpecInfoWrapper> SpecInfos { get; }

            public bool Success { get; } = true;
        }

        private class SpecInfoWrapper {
            private readonly SpecInfo _specInfo;

            public SpecInfoWrapper(SpecInfo specInfo, int fileId) {
                _specInfo = specInfo;
                FileId = fileId;
            }

            public int FileId { get; }
            public Complex First => _specInfo.First;

            public int Number => _specInfo.Number;
        }
    }
}