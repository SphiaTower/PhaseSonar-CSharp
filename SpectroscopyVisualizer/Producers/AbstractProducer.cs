using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Producers
{
    public abstract class AbstractProducer : IProducer
    {
        private bool IsOn { get; set; } = true;
        public BlockingCollection<double[]> BlockingQueue { get; } = new BlockingCollection<double[]>(24);
        // todo config
        public int HistoryProductCnt { get; private set; }

        public void Produce()
        {
            Task.Run((Action) DoInBackground);
        }

        public void Stop()
        {
            IsOn = false;
        }

        public void Reset()
        {
            HistoryProductCnt = 0;
            double[] disposed;
            while (BlockingQueue.TryTake(out disposed))
            {
            }
        }

        protected abstract void Wait();

        protected abstract double[] RetrieveData();

        protected virtual void DoInBackground()
        {
            IsOn = true;
            while (IsOn)
            {
                Wait();
                if (!IsOn) break;
                try
                {
                    var record = RetrieveData();
                    BlockingQueue.Add(record);
                    HistoryProductCnt++;
                }
                catch (Exception e)
                {
                    // todo
                }
            }
            // _sampler.Release();
        }
    }
}