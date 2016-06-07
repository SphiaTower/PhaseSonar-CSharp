using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    /// An interface declared for data saving.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWriter<in T>
    {
        /// <summary>
        /// The state of the Writer.
        /// </summary>
        bool IsOn { get; set; }
        
        /// <summary>
        /// Save data.
        /// </summary>
        /// <param name="data"></param>
        void Enqueue(T data);
    }

    /// <summary>
    /// A base implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Writer<T> : IWriter<T>
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="on">On or off.</param>
        protected Writer(bool on)
        {
            IsOn = on;
        }

        /// <summary>
        /// The queue storing data to be saved.
        /// </summary>
        protected ConcurrentQueue<T> Queue { get; } = new ConcurrentQueue<T>();
        /// <summary>
        /// The state whether the Writer is saving data.
        /// </summary>
        protected bool IsProcessing { get; set; }

        /// <summary>
        /// The state of the Writer.
        /// </summary>
        public bool IsOn { get; set; }

        /// <summary>
        /// Enqueue a new data to save.
        /// </summary>
        /// <param name="data"></param>
        public void Enqueue(T data)
        {
            if (!IsOn)
            {
                throw new Exception("Writer is off");
            }
            Queue.Enqueue(data);

            if (IsProcessing) return;
            IsProcessing = true;
            Task.Run(() =>
            {
                T dequeue;
                while (Queue.TryDequeue(out dequeue))
                {
                    ConsumeElement(dequeue);
                }
                IsProcessing = false;
            });
        }

        /// <summary>
        /// Save data element in the queue.
        /// </summary>
        /// <param name="dequeue">The dequeued element</param>
        protected abstract void ConsumeElement(T dequeue);
    }
}