using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Writers {
    public class SerialWriterV2<T> : IWriterV2<T> {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        /// <summary>
        ///     The queue storing data to be saved.
        /// </summary>
        private volatile bool _isProcessing;

        public event Action<T> ElementDequeued;

        /// <summary>
        ///     Enqueue a new data to save.
        /// </summary>
        /// <param name="data"></param>
        public void Write(T data) {
            _queue.Enqueue(data);

            if (_isProcessing) return;
            _isProcessing = true;
            Task.Run(() => {
                T dequeue;
                while (_queue.TryDequeue(out dequeue)) {
                    ElementDequeued?.Invoke(dequeue);
                }
                _isProcessing = false;
            });
        }
    }
}