using System;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers {
    /// <summary>
    ///     A fake producer which is used by developers.
    /// </summary>
    public class DummyProducer : AbstractProducer<SampleRecord> {
        private readonly double[] _backup;

        private readonly double[] _pulse;
        private readonly Random _random = new Random();

        /// <summary>
        ///     Create an instance.
        /// </summary>
        public DummyProducer() : base(24) // todo
        {
            _pulse = Toolbox.Read(@"C:\Buffer\pulse-5.txt");
            _backup = new double[_pulse.Length];
            Array.Copy(_pulse, _backup, _pulse.Length);
        }

        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected override void OnPreRetrieve() {
        }

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        protected override SampleRecord RetrieveData() {
            var r = _random.NextDouble() + 0.5;
            for (var i = 0; i < _pulse.Length/8; i++) {
                _pulse[i] = _backup[i]*r;
            }
//            double[] array = new double[_backup.Length];
//            Array.Copy(_backup,array,array.Length);
            return new SampleRecord(_pulse, ProductCnt);
        }
    }
}