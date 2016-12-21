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

        public int? MaxCapacity { get; }
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
        public event Action<Exception> ProductionFailed;
        public event Action<T> NewProduct;

        private Exception _allException;
        private void DoInBackground() {
            while (!_cts.IsCancellationRequested) {
                T data;

                try {
                    data = _dataRetriever.TryRetrieveData();
                } catch (Exception e) {
                    if (_allException==null) {
                        _allException = e;
                    } else {
                        _allException = new  Exception(_allException.Message+"\n======================================\n"+e.Message);

                    }
                    _continuousFailCnt++;
                    if (_continuousFailCnt == 10) {
                        ProductionFailed?.Invoke(_allException);
                        break;
                    } else {
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                NewProduct?.Invoke(data);
                _continuousFailCnt = 0;
                if (_cts.IsCancellationRequested) break;
                BlockingQueue.Add(data); // blocking method
                ProductCnt++;
                if (ProductCnt >= TargetCnt.GetValueOrDefault(int.MaxValue)) {
                    HitTarget?.Invoke();
                    break;
                }
            }
        }

        public interface IDataRetriever {
            /// <summary>
            /// 
            /// </summary>
            /// <exception cref="Exception"></exception>
            /// <returns></returns>
            T TryRetrieveData();
        }
    }
}