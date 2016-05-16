using FTIR.Utils;
using JetBrains.Annotations;
using Shokouki.Consumers;
using Shokouki.Producers;

namespace Shokouki.Controllers
{
    public class Scheduler
    {
        private readonly StopWatch _stopWatch = new StopWatch();

        public Scheduler(IProducer producer, UiConsumer<double[]> consumer)
        {
            Producer = producer;
            Consumer = consumer;
        }

        [NotNull]
        public IProducer Producer { get; protected set; }

        [NotNull]
        public UiConsumer<double[]> Consumer { get; protected set; }

        public void Start()
        {
            _stopWatch.Reset();
            Producer.Produce();
            Consumer.Consume();
        }


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