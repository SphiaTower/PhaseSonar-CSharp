using System.Collections.Concurrent;

namespace SpectroscopyVisualizer.Producers
{
    public interface IProducer<T>
    {
        BlockingCollection<T> BlockingQueue { get; }
        int HistoryProductCnt { get; }
        void Produce();
        void Stop();
        void Reset();
    }
}