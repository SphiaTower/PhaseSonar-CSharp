using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTIR.Analyzers;

namespace Shokouki.Controllers {
    public abstract class Camera
    {
        protected ConcurrentQueue<SpecInfo> Queue { get; } = new ConcurrentQueue<SpecInfo>();
        public bool IsProcessing { get; protected set; } = false;
        public bool IsOn { get; set; } = false;
        public void Capture(SpecInfo specInfo)
        {
            if (!IsOn)
            {
                throw new Exception("camera is off");
            }
            Queue.Enqueue(specInfo);

            if (IsProcessing) return;
            IsProcessing = true;
            Task.Run(() =>
            {
                SpecInfo dequeue;
                while (Queue.TryDequeue(out dequeue))
                {
                    Consume(dequeue);
                }
                IsProcessing = false;
            });
        }

        protected abstract void Consume(SpecInfo dequeue);
    }
}
