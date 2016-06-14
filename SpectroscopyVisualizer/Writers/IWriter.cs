namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    ///     An interface declared for data saving.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWriter<in T>
    {
        /// <summary>
        ///     The state of the Writer.
        /// </summary>
        bool IsOn { get; set; }

        /// <summary>
        ///     Save data.
        /// </summary>
        /// <param name="data"></param>
        void Enqueue(T data);
    }
}