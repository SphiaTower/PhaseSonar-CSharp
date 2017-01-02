using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhaseSonar.Analyzers.WithoutReference;

namespace SpectroscopyVisualizer.Consumers {
    public class ParallelConsumerV2<TProduct, TWorker, TResult> : IConsumerV2 where TResult : IResult {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Func<TProduct, TWorker, TResult> _processFunc;

        private readonly BlockingCollection<TProduct> _queue;
        private readonly Action<TResult> _syncResultHandleFunc;
        private readonly int _waitProducerMsTimeout;

        /// <summary>
        ///     A collection of workers consuming the products in parallel.
        /// </summary>
        private readonly IEnumerable<TWorker> _workers;

        private int _continuousFailCnt;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ParallelConsumerV2(BlockingCollection<TProduct> queue, IEnumerable<TWorker> workers,
            Func<TProduct, TWorker, TResult> processFunc, Action<TResult> syncResultHandleFunc,
            int waitProducerMsTimeout, int? targetCnt) {
            _processFunc = processFunc;
            _queue = queue;
            _waitProducerMsTimeout = waitProducerMsTimeout;
            _syncResultHandleFunc = syncResultHandleFunc;
            _workers = workers;
            TargetCnt = targetCnt;
        }

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt { get; private set; }

        public int? TargetCnt { get; }


        /// <summary>
        ///     Stop consuming.
        /// </summary>
        public void Stop() {
            _cts.Cancel();
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public void Start() {
            Task.Run(() => {
                var empty = false;
                var parallelOptions = new ParallelOptions {CancellationToken = _cts.Token};
                try {
                    Parallel.ForEach(_workers, parallelOptions, worker => {
                        TProduct raw;
                        while (_queue.TryTake(out raw, _waitProducerMsTimeout)) {
                            parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                            var result = ConsumeElement(raw, worker);
                            lock (this) {
                                parallelOptions.CancellationToken.ThrowIfCancellationRequested();
                                _syncResultHandleFunc(result);
                                ConsumedCnt++;
                                Update?.Invoke(result);
                                if (result.IsSuccessful) {
                                    _continuousFailCnt = 0;
                                } else {
                                    _continuousFailCnt++;
                                    if (_continuousFailCnt >= 10) {
                                        SourceInvalid?.Invoke();
                                        return;
                                    }
                                }

                                if (ConsumedCnt >= TargetCnt.GetValueOrDefault(int.MaxValue)) {
                                    TargetAmountReached?.Invoke();
                                    return;
                                }
                            }
                        }
                        empty = true;
                    });
                    if (empty) {
                        ProducerEmpty?.Invoke();
                    }
                } catch (OperationCanceledException) {
                    // tasks stopped
                }
            });
        }


        public event Action SourceInvalid;
        public event UpdateEventHandler Update;
        public event Action ProducerEmpty;
        public event Action TargetAmountReached;

        private TResult ConsumeElement(TProduct raw, TWorker worker) {
            return _processFunc(raw, worker);
        }
    }

    public interface IResult {
        bool IsSuccessful { get; }
        bool HasException { get; }
        ProcessException? Exception { get; }
        int ExceptionCnt { get; }
        int ValidPeriodCnt { get; }
    }
}