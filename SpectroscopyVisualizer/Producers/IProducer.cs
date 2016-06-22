using System.Collections.Concurrent;

namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     An interface for producers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProducer<T>
    {
        /// <summary>
        ///     The queue containing all products.
        /// </summary>
        BlockingCollection<T> BlockingQueue { get; }

        /// <summary>
        ///     The count of product.
        /// </summary>
        int HistoryProductCnt { get; }

        /// <summary>
        ///     Start Producing.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop Producing.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Reset the status of the producer.
        /// </summary>
        void Reset();
    }
}