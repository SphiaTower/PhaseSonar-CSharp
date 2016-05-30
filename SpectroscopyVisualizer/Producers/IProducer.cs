using System.Collections.Concurrent;

namespace SpectroscopyVisualizer.Producers
{
    public interface IProducer
    {
        BlockingCollection<double[]> BlockingQueue { get; }
        int HistoryProductCnt { get; }
        void Produce();
        void Stop();
        void Reset();
    }
}