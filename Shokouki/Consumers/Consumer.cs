using System.Collections.Concurrent;
using System.Threading.Tasks;
using FTIR.Utils;
using JetBrains.Annotations;

namespace Shokouki.Consumers
{
    public interface IConsumer
    {
        int ConsumedCnt { get; }
        void Stop();
        void Consume();
        void Reset();
    }

    public abstract class Consumer<T> : IConsumer
    {
        protected Consumer([NotNull] BlockingCollection<T> blockingQueue)
        {
            Toolbox.RequireNonNull(blockingQueue, "blockingQueue");
            BlockingQueue = blockingQueue;
        }

        public int MillisecondsTimeout { get; set; } = 10000;

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
                    if (!BlockingQueue.TryTake(out raw, MillisecondsTimeout)) break;
                    if (!IsOn) return;
                    ConsumeElement(raw);
                    ConsumedCnt++;
                }
                IsOn = false;
            });
        }

        public int ConsumedCnt { get; protected set; }

        public virtual void Reset()
        {
            ConsumedCnt = 0;
        }

        public abstract void ConsumeElement([NotNull] T item);
    }
}