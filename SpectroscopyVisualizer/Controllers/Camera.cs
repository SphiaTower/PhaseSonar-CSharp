using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Controllers
{
    public interface ICamera<in T>
    {
        bool IsOn { get; set; }
        bool IsProcessing { get; }
        void Capture(T data);
    }

    public abstract class Camera<T> : ICamera<T>
    {
        protected Camera(bool on)
        {
            IsOn = on;
        }

        protected ConcurrentQueue<T> Queue { get; } = new ConcurrentQueue<T>();
        public bool IsProcessing { get; protected set; }
        public bool IsOn { get; set; }

        public void Capture(T data)
        {
            if (!IsOn)
            {
                throw new Exception("camera is off");
            }
            Queue.Enqueue(data);

            if (IsProcessing) return;
            IsProcessing = true;
            Task.Run(() =>
            {
                T dequeue;
                while (Queue.TryDequeue(out dequeue))
                {
                    ConsumeElement(dequeue);
                }
                IsProcessing = false;
            });
        }

        protected abstract void ConsumeElement(T dequeue);
    }
}