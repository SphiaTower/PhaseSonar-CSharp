namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    /// An interface defined for all consumers.
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        /// The number of elements have been consumed.
        /// </summary>
        int ConsumedCnt { get; }
        /// <summary>
        /// Stop consuming.
        /// </summary>
        void Stop();
        /// <summary>
        /// Start consuming.
        /// </summary>
        void Consume();
        /// <summary>
        /// Reset the state of the consumer.
        /// </summary>
        void Reset();
        
    }
}