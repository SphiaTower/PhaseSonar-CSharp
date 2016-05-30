using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;
using PhaseSonar.Utils;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Consumers
{
    public abstract class Consumer<T> : IConsumer
    {
        public int ContinuousFailCnt { get; protected set; }

        protected Consumer([NotNull] BlockingCollection<T> blockingQueue)
        {
            Toolbox.RequireNonNull(blockingQueue, "blockingQueue");
            BlockingQueue = blockingQueue;
        }
        public delegate void ConsumerFailEventHandler(object sender);
        public delegate void ElementConsumedEventHandler(object sender);

        public event ConsumerFailEventHandler FailEvent;
        public event ElementConsumedEventHandler ConsumeEvent;

        public int MillisecondsTimeout { get; set; } = 10000;

        protected BlockingCollection<T> BlockingQueue { get; }

        protected bool IsOn { get; set; }

        public void Stop()
        {
            IsOn = false;
        }

        public int ConsumedCnt { get; protected set; }

        public virtual void Reset()
        {
            ConsumedCnt = 0;
            ContinuousFailCnt = 0;
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
                    if (ConsumeElement(raw))
                    {
                        ContinuousFailCnt = 0;
                        ConsumedCnt++;
                        FireConsumeEvent();
                    }
                    else
                    {
                        ContinuousFailCnt++;
                        if (ContinuousFailCnt >= 10)
                        {
                            FireFailEvent();
//                            Application.Current.Dispatcher.InvokeAsync(()=>onConsumeFailed?.Invoke());
                            break;
                        }
                    }
                }
                IsOn = false;
            });
        }

        protected void FireFailEvent()
        {
            FailEvent?.Invoke(this);
        }

        protected void FireConsumeEvent()
        {
            ConsumeEvent?.Invoke(this);
        }

        public abstract bool ConsumeElement([NotNull] T item);
    }
}