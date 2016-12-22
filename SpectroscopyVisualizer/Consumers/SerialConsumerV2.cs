using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PhaseSonar.Analyzers;

namespace SpectroscopyVisualizer.Consumers {
    public class SerialConsumerV2<TProduct> : IConsumerV2 {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Func<TProduct, IResult> _processFunc;
        private readonly BlockingCollection<TProduct> _queue;
        private readonly int _waitProducerTimeoutMs;
        private int _continuousFailCnt;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SerialConsumerV2(Func<TProduct, IResult> processFunc, BlockingCollection<TProduct> queue, int? targetCnt,
            int waitProducerTimeoutMs) {
            _processFunc = processFunc;
            _waitProducerTimeoutMs = waitProducerTimeoutMs;
            _queue = queue;
            TargetCnt = targetCnt;
        }

        public Dictionary<ProcessException, int> Exceptions { get; } = new Dictionary<ProcessException, int>();

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt { get; private set; }

        public int? TargetCnt { get; }

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            _cancellationTokenSource.Cancel();
        }


        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            Task.Run(() => {
                while (!_cancellationTokenSource.IsCancellationRequested) {
                    Thread.Sleep(100);
                    TProduct raw;
                    if (!_queue.TryTake(out raw, _waitProducerTimeoutMs)) {
                        ProducerEmpty?.Invoke();
                        break;
                    }
                    if (_cancellationTokenSource.IsCancellationRequested) return;
                    var result = ConsumeElement(raw);
                    Update?.Invoke(result);
                    if (result.IsSuccessful) {
                        _continuousFailCnt = 0;
                    } else {
                        _continuousFailCnt++;
                        if (_continuousFailCnt >= 10) {
                            SourceInvalid?.Invoke();
                            break;
                        }
                    }
                    ConsumedCnt++;
                    if (ConsumedCnt >= TargetCnt.GetValueOrDefault(int.MaxValue)) {
                        TargetAmountReached?.Invoke();
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        public event Action SourceInvalid;
        public event UpdateEventHandler Update;
        public event Action ProducerEmpty;
        public event Action TargetAmountReached;

        [NotNull]
        private IResult ConsumeElement(TProduct raw) {
            return _processFunc(raw);
        }
    }
}