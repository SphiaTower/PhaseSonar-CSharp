using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FTIR.Utils;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;

namespace Shokouki.Model
{
    public interface IProducer
    {
        BlockingCollection<double[]> BlockingQueue { get; }
        int HistoryProductCnt { get; }
        void Produce();
        void Stop();
        void Reset();
    }

    public abstract class ProducerImpl : IProducer
    {
        private bool IsOn { get; set; } = true;
        public BlockingCollection<double[]> BlockingQueue { get; } = new BlockingCollection<double[]>(16);
        public int HistoryProductCnt { get; private set; }

        public void Produce()
        {
            Task.Run((Action) DoInBackground);
        }

        public void Stop()
        {
            IsOn = false;
        }

        public void Reset()
        {
            HistoryProductCnt = 0;
            double[] disposed;
            while (BlockingQueue.TryTake(out disposed))
            {
            }
        }

        protected abstract void Wait();

        protected abstract double[] RetrieveData();

        protected virtual void DoInBackground()
        {
            IsOn = true;
            while (IsOn)
            {
                Wait();
                if (!IsOn) break;
                try
                {
                    var record = RetrieveData();
                    BlockingQueue.Add(record);
                    HistoryProductCnt++;
                }
                catch (Exception e)
                {
                    // todo
                }
            }
            // _sampler.Release();
        }
    }

    public class SampleProducer : ProducerImpl
    {
        private readonly Sampler _sampler;

        public SampleProducer(Sampler sampler)
        {
            _sampler = sampler;
        }

        protected override void Wait()
        {
            while (_sampler.Status() != ScopeAcquisitionStatus.Complete
                   || BlockingQueue.Count == BlockingQueue.BoundedCapacity)
            {
                Thread.Sleep(1000); // todo
            }
        }

        protected override double[] RetrieveData()
        {
            return _sampler.Retrieve();
        }

        //todo
    }

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
            var r = _random.NextDouble()*2;
            for (var i = 0; i < _pulse.Length; i++)
            {
                _pulse[i] = _backup[i]*r;
            }
            return _pulse;
        }
    }
}