using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     A consumer which takes and consumes products in parallel.
    /// </summary>
    /// <typeparam name="TProduct"></typeparam>
    /// <typeparam name="TWorker"></typeparam>
    public abstract class ParallelConsumer<TProduct, TWorker> : AbstractConsumer<TProduct>
    {
        /// <summary>
        ///     Create a Consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue which containing elements to be consumed</param>
        /// <param name="workers">A collection of workers consuming the products in parallel.</param>
        protected ParallelConsumer([NotNull] BlockingCollection<TProduct> blockingQueue,
            [NotNull] IEnumerable<TWorker> workers) : base(blockingQueue)
        {
            Workers = workers;
        }

        /// <summary>
        ///     A collection of workers consuming the products in parallel.
        /// </summary>
        [NotNull]
        protected IEnumerable<TWorker> Workers { get; }

        /// <summary>
        ///     Start consuming.
        /// </summary>
        public override void Consume()
        {
            IsOn = true;
            Task.Run(() =>
            {
                Parallel.ForEach(Workers, worker =>
                {
                    while (IsOn)
                    {
                        TProduct raw;
                        if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) break;
                        if (!IsOn) return;
                        if (ConsumeElement(raw, worker))
                        {
                            lock (this)
                            {
                                ContinuousFailCnt = 0;
                                ConsumedCnt++;
                                FireConsumeEvent();
                            }
                        }
                        else
                        {
                            lock (this)
                            {
                                ContinuousFailCnt++;
                                if (ContinuousFailCnt >= 10)
                                {
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

        /// <summary>
        ///     Consume element in a branch.
        /// </summary>
        /// <param name="element">The product</param>
        /// <param name="worker">The worker in this branch.</param>
        /// <returns>Whether consuming succeeds.</returns>
        protected abstract bool ConsumeElement(TProduct element, TWorker worker);
    }
}