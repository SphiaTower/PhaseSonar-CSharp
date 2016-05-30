using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows;
using PhaseSonar.Utils;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Consumers
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
        private int _continuousFailCnt;

        protected Consumer([NotNull] BlockingCollection<T> blockingQueue)
        {
            Toolbox.RequireNonNull(blockingQueue, "blockingQueue");
            BlockingQueue = blockingQueue;
        }
        public delegate void ConsumerFailEventHandler(object sender);

        public event ConsumerFailEventHandler FailEvent;

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
            _continuousFailCnt = 0;
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
                        ConsumedCnt++;
                        _continuousFailCnt = 0;
                    }
                    else
                    {
                        _continuousFailCnt++;
                        if (_continuousFailCnt >= 10)
                        {
                            FailEvent?.Invoke(this);
//                            Application.Current.Dispatcher.InvokeAsync(()=>onConsumeFailed?.Invoke());
                            break;
                        }
                    }
                }
                IsOn = false;
            });
        }

        public abstract bool ConsumeElement([NotNull] T item);
    }
}