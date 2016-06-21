using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Consumers {
    public abstract class ParallelConsumer<TProduct,TWorker>:AbstractConsumer<TProduct> {
        /// <summary>
        ///     Create a Consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue which containing elements to be consumed</param>
        protected ParallelConsumer([NotNull] BlockingCollection<TProduct> blockingQueue, [NotNull]IEnumerable<TWorker> workers) : base(blockingQueue)
        {
            Workers = workers;
        }
        [NotNull]
        protected IEnumerable<TWorker> Workers { get; }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public override void Consume() {
            IsOn = true;
            Task.Run(() => {
                Parallel.ForEach(Workers, worker => {
                    while (IsOn) {
                        TProduct raw;
                        if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) break;
                        if (!IsOn) return;
                        if (ConsumeElement(raw, worker)) {
                            lock (this) {
                                ContinuousFailCnt = 0;
                                ConsumedCnt++;
                                FireConsumeEvent();
                            }
                        } else {
                            lock (this) {
                                ContinuousFailCnt++;
                                if (ContinuousFailCnt >= 10) {
                                    FireFailEvent();
                                    break;
                                }
                            }
                        }
                    }
                    IsOn = false;
                });
            });
        }

        protected abstract bool ConsumeElement(TProduct raw, TWorker worker);

    }
}
