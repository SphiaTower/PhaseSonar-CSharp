namespace SpectroscopyVisualizer.Consumers
{
    /// <summary>
    ///     An interface for all consumers.
    /// </summary>
    public interface IConsumer
    {
        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        int ConsumedCnt { get; }

        /// <summary>
        ///     Stop consuming.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Start consuming.
        /// </summary>
        void Start();

    }
}