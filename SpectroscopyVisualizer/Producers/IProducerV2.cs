using System;
using System.Collections.Concurrent;

namespace SpectroscopyVisualizer.Producers {
    public interface IProducerV2<T> {
        /// <summary>
        ///     The queue containing all products.
        /// </summary>
        BlockingCollection<T> BlockingQueue { get; }

        /// <summary>
        ///     The count of product.
        /// </summary>
        int ProductCnt { get; }

        int? MaxCapacity { get; }

        int? TargetCnt { get; }

        /// <summary>
        ///     Start Producing.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop Producing.
        /// </summary>
        void Stop();

        event Action HitTarget;
        event Action ProductionFailed;
        event Action<T> NewProduct;
    }
}