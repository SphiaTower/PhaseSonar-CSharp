using System;
using FTIR.Utils;

namespace Shokouki.Producers
{
    public class DummyProducer : ProducerImpl
    {
        private readonly Random _random = new Random();
        private readonly double[] _backup;

        private double[] _pulse;

        public DummyProducer()
        {
            _pulse = Toolbox.Read(@"C:\Buffer\pulses\pulse-5.txt");
            _backup = new double[_pulse.Length];
            Array.Copy(_pulse, _backup, _pulse.Length);
        }

        protected override void Wait()
        {
        }

        protected override double[] RetrieveData()
        {
            var r = _random.NextDouble()+0.5;
            for (var i = 0; i < _pulse.Length/8; i++)
            {
                _pulse[i] = _backup[i]*r;
            }
            return _pulse;
        }
    }
}