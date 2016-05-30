namespace SpectroscopyVisualizer.Consumers
{
    public interface IConsumer
    {
        int ConsumedCnt { get; }
        void Stop();
        void Consume();
        void Reset();
        
    }
}