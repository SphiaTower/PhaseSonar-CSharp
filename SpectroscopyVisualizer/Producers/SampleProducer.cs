using System.Threading;
using NationalInstruments.Examples.StreamToDiskConsole;
using NationalInstruments.ModularInstruments.NIScope;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     A producer which retrieves data via an ADC with NI-Scope.
    /// </summary>
    public class SampleProducer : AbstractProducer<SampleRecord>
    {
        private readonly Sampler _sampler;

        /// <summary>
        ///     Create a SampleProducer.
        /// </summary>
        /// <param name="sampler">
        ///     <see cref="Sampler" />
        /// </param>
        /// <param name="writer">
        ///     <see cref="SampleWriter" />
        /// </param>
        public SampleProducer(Sampler sampler, SampleWriter writer)
        {
            Writer = writer;
            _sampler = sampler;
        }

        /// <see cref="SampleWriter" />
        public SampleWriter Writer { get; set; }

        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected override void OnPreRetrieve()
        {
            while (_sampler.Status() != ScopeAcquisitionStatus.Complete||BlockingQueue.Count==BlockingQueue.BoundedCapacity)
            {
                Thread.Sleep(10); 
            }
        }

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        protected override SampleRecord RetrieveData()
        {
            var pulseSequence = _sampler.Retrieve();
            var record = new SampleRecord(pulseSequence, ProductCnt);
            return record;
        }

        protected override void OnDataEnqueued(SampleRecord data)
        {
            base.OnDataEnqueued(data);
            if (Writer.IsOn) Writer.Write(data);
        }
    }
}