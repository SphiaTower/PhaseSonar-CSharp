using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Producers {
    public class SerialProducerV2<T> : IProducerV2<T> {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly IDataRetriever _dataRetriever;

        private int _continuousFailCnt;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SerialProducerV2(IDataRetriever dataRetriever, int? maxCapacity, int? targetCnt) {
            _dataRetriever = dataRetriever;
            MaxCapacity = maxCapacity;
            TargetCnt = targetCnt;
            if (maxCapacity != null) {
                BlockingQueue = new BlockingCollection<T>((int) maxCapacity);
            } else {
                BlockingQueue = new BlockingCollection<T>();
            }
        }

        /// <summary>
        ///     The queue containing all products.
        /// </summary>
        public BlockingCollection<T> BlockingQueue { get; }

        /// <summary>
        ///     The count of product.
        /// </summary>
        public int ProductCnt { get; private set; }

        public int? MaxCapacity { get;}
        public int? TargetCnt { get; }

        /// <summary>
        ///     Start Producing.
        /// </summary>
        public void Start() {
            Task.Run((Action) DoInBackground);
        }

        /// <summary>
        ///     Stop Producing.
        /// </summary>
        public void Stop() {
            _cts.Cancel();
        }

        public event Action HitTarget;
        public event Action ProductionFailed;
        public event Action<T> NewProduct;

        private void DoInBackground() {
            while (!_cts.IsCancellationRequested) {
                T data;
                if (_dataRetriever.TryRetrieveData(out data)) {
                    NewProduct?.Invoke(data);
                    _continuousFailCnt = 0;
                    if (_cts.IsCancellationRequested) break;
                    BlockingQueue.Add(data); // blocking method
                    ProductCnt++;
                    if (ProductCnt >= TargetCnt.GetValueOrDefault(int.MaxValue)) {
                        HitTarget?.Invoke();
                        break;
                    }
                } else {
                    _continuousFailCnt++;
                    if (_continuousFailCnt == 10) {
                        ProductionFailed?.Invoke();
                    }
                }
            }
        }

        public interface IDataRetriever {
            bool TryRetrieveData(out T data);
        }
    }
}