using System.Threading;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;
using SpectroscopyVisualizer.Controllers;

namespace SpectroscopyVisualizer.Producers {
    public class SampleProducer : ProducerBase
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