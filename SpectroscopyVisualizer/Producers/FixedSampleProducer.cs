using System.Windows;
using NationalInstruments.Examples.StreamToDiskConsole;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer.Producers
{
    public class FixedSampleProducer : AbstractProducer<SampleRecord>
    {
        private readonly Sampler _sampler;

        public FixedSampleProducer(Sampler sampler,int boundedCapacity) :base(boundedCapacity)
        {
            _sampler = sampler;
        }

        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected override void OnPreRetrieve()
        {
        }

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        protected override SampleRecord RetrieveData()
        {
            var pulseSequence = _sampler.Retrieve();
            return new SampleRecord(pulseSequence, ProductCnt);
        }

        protected override void OnDataEnqueued(SampleRecord data)
        {
            if (ProductCnt >= BoundedCapacity)
            {
                Stop();
                MessageBox.Show("sampling finished");
            }
        }
    }
}