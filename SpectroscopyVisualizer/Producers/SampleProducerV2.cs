using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;

namespace SpectroscopyVisualizer.Producers {
    public class SampleProducerV2 : IProducerV2<SampleRecord>, SerialProducerV2<SampleRecord>.IDataRetriever {
        private readonly IProducerV2<SampleRecord> _producer;

        private readonly Sampler _sampler;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SampleProducerV2(Sampler sampler, int? maxCapacity, int? targetCnt) {
            _sampler = sampler;
            _producer = new SerialProducerV2<SampleRecord>(this, maxCapacity, targetCnt);
        }

        /// <summary>
        ///     The queue containing all products.
        /// </summary>
        public BlockingCollection<SampleRecord> BlockingQueue => _producer.BlockingQueue;

        /// <summary>
        ///     The count of product.
        /// </summary>
        public int ProductCnt => _producer.ProductCnt;

        public int? MaxCapacity {
            get { return _producer.MaxCapacity; }
            set { _producer.MaxCapacity = value; }
        }


        public int? TargetCnt => _producer.TargetCnt;

        /// <summary>
        ///     Start Producing.
        /// </summary>
        public void Start() {
            _producer.Start();
        }

        /// <summary>
        ///     Stop Producing.
        /// </summary>
        public void Stop() {
            _producer.Stop();
        }

        public event Action HitTarget {
            add { _producer.HitTarget += value; }
            remove { _producer.HitTarget -= value; }
        }

        public event Action ProductionFailed {
            add { _producer.ProductionFailed += value; }
            remove { _producer.ProductionFailed -= value; }
        }

        public event Action<SampleRecord> NewProduct {
            add { _producer.NewProduct += value; }
            remove { _producer.NewProduct -= value; }
        }

        public bool TryRetrieveData(out SampleRecord data) {
            double[] pulseSequence;
            try {
                pulseSequence = _sampler.Retrieve();
            } catch (Exception) {
                data = null;
                return false;
            }
            data = new SampleRecord(pulseSequence, ProductCnt);
            return true;
        }


        [NotNull]
        public SampleRecord RetrieveData() {
            var pulseSequence = _sampler.Retrieve();
            return new SampleRecord(pulseSequence, ProductCnt);
        }
    }
}