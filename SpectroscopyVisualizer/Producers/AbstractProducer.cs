using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     An skeletal implementation of IProducer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractProducer<T> : IProducer<T>
    {
        public int BoundedCapacity { get; set; }

        protected AbstractProducer(int boundedCapacity)
        {
            BoundedCapacity = boundedCapacity;
            BlockingQueue = new BlockingCollection<T>(BoundedCapacity);
        }

        /// <summary>
        ///     Whether the producer is on or off.
        /// </summary>
        protected bool IsOn { get; set; } = true;

        /// <summary>
        ///     The queue containing all products.
        /// </summary>
        public BlockingCollection<T> BlockingQueue { get; } // todo config

        /// <summary>
        ///     The count of product.
        /// </summary>
        public int ProductCnt { get; private set; }

        /// <summary>
        ///     Start Producing.
        /// </summary>
        public void Start()
        {
            Task.Run((Action) DoInBackground);
        }

        /// <summary>
        ///     Stop Producing.
        /// </summary>
        public void Stop()
        {
            IsOn = false;
        }


        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected abstract void OnPreRetrieve();

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        protected abstract T RetrieveData();

        /// <summary>
        ///     The main flow of producer, executed in a backgound thread.
        /// </summary>
        protected virtual void DoInBackground()
        {
            while (IsOn)
            {
                OnPreRetrieve();
                T data;
                try
                {
                    data = RetrieveData();
                }
                catch (Exception ignored)
                {
                    continue;
                }
                if (!IsOn) break;
                BlockingQueue.Add(data); // blocking method
                ProductCnt++;
                OnDataEnqueued(data);
            }
        }

        protected virtual void OnDataEnqueued(T data)
        {
            
        }

    }
}