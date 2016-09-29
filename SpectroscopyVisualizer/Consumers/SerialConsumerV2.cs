using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SpectroscopyVisualizer.Consumers {
    public class SerialConsumerV2<TProduct> :IConsumerV2{
        private BlockingCollection<TProduct> _queue= new BlockingCollection<TProduct>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private int _waitProducerTimeoutMs;
        private int _continuousFailCnt=0;

        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        public int ConsumedCnt { get; private set; }
        public int TargetCnt { get; }

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
            var task = Task.Run(() => {
                while (!_cancellationTokenSource.IsCancellationRequested) {
                    TProduct raw;
                    if (!_queue.TryTake(out raw, _waitProducerTimeoutMs)) {
                        ProducerEmpty?.Invoke();
                        break;
                    }
                    if (!_cancellationTokenSource.IsCancellationRequested) return;
                    if (ConsumeElement(raw)) {
                        _continuousFailCnt = 0;
                        ElementConsumedSuccessfully?.Invoke();
                    } else {
                        _continuousFailCnt++;
                        if (_continuousFailCnt >= 10) {
                            SourceInvalid?.Invoke();
                            break;
                        }
                    }
                    ConsumedCnt++;
                    if (ConsumedCnt==TargetCnt) {
                        TargetAmountReached?.Invoke();
                    }
                }
            },_cancellationTokenSource.Token);
            task.Start();
        }

        private bool ConsumeElement(TProduct raw) {
            throw new NotImplementedException();
        }

        public event Action SourceInvalid;
        public event Action ElementConsumedSuccessfully;
        public event Action ProducerEmpty;
        public event Action TargetAmountReached;
    }
}
