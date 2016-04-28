using System.Windows;
using FTIR.Utils;
using Shokouki.Consumers;
using Shokouki.Model;
using Shokouki.Presenters;

namespace Shokouki.Controllers
{
    public class Scheduler
    {
        private readonly StopWatch _stopWatch = new StopWatch();

        public Scheduler(IScopeView view)
        {
//            Producer = new SampleProducer(Injector.NewSampler());
            Producer = new DummyProducer();
            Consumer = new SpectroscopyVisualizer(
                Producer.BlockingQueue,
                view,
                Injector.NewAccumulator(),
                Injector.NewAdapter(view));
        }

        public IProducer Producer { get; protected set; }
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
            var timeElapsed = _stopWatch.Reset();

            var historyProductCnt = Producer.HistoryProductCnt;
            var productInQueue = Producer.BlockingQueue.Count;
            var productConsumed = historyProductCnt - productInQueue;

            MessageBox.Show(
                nameof(historyProductCnt) + ": " + historyProductCnt + '\n'
                + nameof(productInQueue) + ": " + productInQueue + '\n'
                + nameof(productConsumed) + ": " + productConsumed + '\n'
                + nameof(timeElapsed) + ": " + timeElapsed
                , "sampling stopped", MessageBoxButton.OK);


            Producer.Reset();
            Consumer.Reset();
        }
    }
}