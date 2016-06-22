using JetBrains.Annotations;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer
{
    /// <summary>
    ///     A scheduler scheduling the producer and the consumer.
    /// </summary>
    public class Scheduler
    {
        /// <summary>
        ///     Create a scheduler.
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="consumer"></param>
        public Scheduler(IProducer<SampleRecord> producer, AbstractConsumer<SampleRecord> consumer)
        {
            Producer = producer;
            Consumer = consumer;
        }

        /// <summary>
        ///     A stopwatch.
        /// </summary>
        public StopWatch Watch { get; } = new StopWatch();

        /// <summary>
        ///     The producer
        /// </summary>
        [NotNull]
        public IProducer<SampleRecord> Producer { get; protected set; }

        /// <summary>
        ///     The consumer
        /// </summary>
        [NotNull]
        public AbstractConsumer<SampleRecord> Consumer { get; protected set; }

        /// <summary>
        ///     Start the system.
        /// </summary>
        public void Start()
        {
            Watch.Reset();
            Producer.Start();
            Consumer.Consume();
        }


        /// <summary>
        ///     Stop the system.
        /// </summary>
        public void Stop()
        {
            Producer.Stop();
            Consumer.Stop();
            /* var timeElapsed = _stopWatch.Reset();

            var historyProductCnt = Producer.HistoryProductCnt;
            var productInQueue = Producer.BlockingQueue.Count;
            var productConsumed = historyProductCnt - productInQueue;

            MessageBox.Show(
                nameof(historyProductCnt) + ": " + historyProductCnt + '\n'
                + nameof(productInQueue) + ": " + productInQueue + '\n'
                + nameof(productConsumed) + ": " + productConsumed + '\n'
                + nameof(timeElapsed) + ": " + timeElapsed
                , "sampling stopped", MessageBoxButton.OK);
*/
            Producer.Reset();
            Consumer.Reset();
        }
    }
}