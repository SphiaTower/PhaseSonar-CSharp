using System.Collections.Concurrent;
using System.Threading.Tasks;
using FTIR.Utils;
using JetBrains.Annotations;

namespace Shokouki.Consumers
{
    public interface IConsumer
    {
        void Stop();
        void Consume();
        int ConsumedCnt { get; }
        void Reset();
    }

    public abstract class Consumer<T> : IConsumer
    {
        public int MillisecondsTimeout { get; set; } = 10000;

        protected Consumer([NotNull]BlockingCollection<T> blockingQueue)
        {
            Toolbox.RequireNonNull(blockingQueue, "blockingQueue");
            BlockingQueue = blockingQueue;
        }

        protected BlockingCollection<T> BlockingQueue { get; }

        protected bool IsOn { get; set; }

        public void Stop()
        {
            IsOn = false;
        }

        public virtual void Consume()
        {
            IsOn = true;
            Task.Run(() =>
            {
                while (IsOn)
                {
                    T raw;
                    if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) return;
                    if (!IsOn) return;
                    ConsumeElement(raw);
                    ConsumedCnt++;
                }
                IsOn = false;
            });
        }

        public int ConsumedCnt { get; protected set; } = 0;
        public virtual void Reset()
        {
            ConsumedCnt = 0;
        }

        public abstract void ConsumeElement([NotNull]T item);
    }
}