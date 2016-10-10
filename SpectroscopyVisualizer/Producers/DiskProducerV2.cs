using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers {
    public class DiskProducerV2 : IProducerV2<SampleRecord>, SerialProducerV2<SampleRecord>.IDataRetriever {
        private readonly IEnumerator<string> _enumerator;
        private readonly SerialProducerV2<SampleRecord> _producer;

        [NotNull] private readonly Func<string, double[]> _readFileFunc;

        private readonly Regex _regex = new Regex("\\[No-(.+)\\]");

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public DiskProducerV2([NotNull] IReadOnlyCollection<string> paths, bool isCompressed) {
            _producer = new SerialProducerV2<SampleRecord>(this, paths.Count, paths.Count);
            if (isCompressed) {
                _readFileFunc = Toolbox.DeserializeData<double[]>;
            } else {
                _readFileFunc = Toolbox.ReadMemoryEfficiently;
            }
            _enumerator = paths.GetEnumerator();
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

        [NotNull]
        public SampleRecord RetrieveData() {
            if (_enumerator.MoveNext()) {
                var path = _enumerator.Current;
                var match = _regex.Match(path);
                int num;
                if (!int.TryParse(match.Value, out num)) {
                    num = ProductCnt;
                }
                return CreateRecord(path, num);
            }
            throw new InvalidOperationException("iteration should have been stopped");
        }

        [NotNull]
        private SampleRecord CreateRecord(string path, int num) {
            return new SampleRecord(_readFileFunc(path), num);
        }
    }
}