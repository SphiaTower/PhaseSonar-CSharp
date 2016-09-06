using System.Collections.Concurrent;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Consumers {
    /// <summary>
    ///     A serial consumer which takes products out one by one.
    /// </summary>
    /// <typeparam name="TProduct"></typeparam>
    public abstract class SerialConsumer<TProduct> : AbstractConsumer<TProduct> {
        /// <summary>
        ///     Create a Consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue which containing elements to be consumed</param>
        protected SerialConsumer([NotNull] BlockingCollection<TProduct> blockingQueue) : base(blockingQueue) {
        }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public override void Start() {
            IsOn = true;
            Task.Run(() => {
                while (IsOn) {
                    TProduct raw;
                    if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) {
                        FireNoProductEvent();
                        break;
                    }
                    if (!IsOn) return;
                    if (ConsumeElement(raw)) {
                        ContinuousFailCnt = 0;
                        ConsumedCnt++;
                        FireConsumeEvent();
                    }
                    else {
                        ContinuousFailCnt++;
                        if (ContinuousFailCnt >= 10) {
                            FireFailEvent();
                            break;
                        }
                    }
                }
                IsOn = false;
            });
        }

        /// <summary>
        ///     Comsume an element dequeued from the queue.
        /// </summary>
        /// <param name="record">The element</param>
        /// <returns>Consumed successfully or not</returns>
        public abstract bool ConsumeElement([NotNull] TProduct record);
    }
}