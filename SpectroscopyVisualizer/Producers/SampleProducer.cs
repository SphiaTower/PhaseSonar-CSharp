using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;
using SpectroscopyVisualizer.Controllers;

namespace SpectroscopyVisualizer.Producers {
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

        public SampleProducer(Sampler sampler, SampleCamera camera)
        {
            Camera = camera;
            _sampler = sampler;
        }

        public SampleCamera Camera { get; set; }

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
            var pulseSequence = _sampler.Retrieve();
            if (Camera.IsOn) Camera.Capture(pulseSequence);
            return pulseSequence;
        }

        //todo
    }
}