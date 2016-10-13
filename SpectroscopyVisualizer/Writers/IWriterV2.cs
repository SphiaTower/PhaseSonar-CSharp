using System;

namespace SpectroscopyVisualizer.Writers {
    public interface IWriterV2<T> {
        /// <summary>
        ///     Enqueue the data to save.
        /// </summary>
        /// <param name="data"></param>
        void Write(T data);

        event Action<T> ElementDequeued;
    }
}