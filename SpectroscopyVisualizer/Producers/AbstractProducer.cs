using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Producers
{
    public abstract class AbstractProducer<T> : IProducer<T>
    {
        private bool IsOn { get; set; } = true;
        public BlockingCollection<T> BlockingQueue { get; } = new BlockingCollection<T>(24);
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
            T disposed;
            while (BlockingQueue.TryTake(out disposed))
            {
            }
        }

        protected abstract void Wait();

        protected abstract T RetrieveData();

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