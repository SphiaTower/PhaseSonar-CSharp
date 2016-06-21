using System.Threading;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Producers
{
    public class SampleProducer : AbstractProducer<SampleRecord>
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
                Thread.Sleep(10); // todo
            }
        }

        protected override SampleRecord RetrieveData()
        {
            var pulseSequence = _sampler.Retrieve();
            SampleRecord record = new SampleRecord(pulseSequence,HistoryProductCnt);
            if (Writer.IsOn) Writer.Write(record);
            return record;
        }

    }
}