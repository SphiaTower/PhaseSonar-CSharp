using System.Threading;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Producers
{
    public class SampleProducer : AbstractProducer
    {
        private readonly Sampler _sampler;

        public SampleProducer(Sampler sampler, SampleWriter writer)
        {
            Writer = writer;
            _sampler = sampler;
        }

        public SampleWriter Writer { get; set; }

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
            if (Writer.IsOn) Writer.Enqueue(pulseSequence);
            return pulseSequence;
        }

        //todo
    }
}