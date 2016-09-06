namespace SpectroscopyVisualizer.Writers {
    /// <summary>
    ///     An interface declared for data saving.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWriter<in T> {
        /// <summary>
        ///     The state of the Writer.
        /// </summary>
        bool IsOn { get; set; }

        /// <summary>
        ///     Enqueue the data to save.
        /// </summary>
        /// <param name="data"></param>
        void Write(T data);
    }
}