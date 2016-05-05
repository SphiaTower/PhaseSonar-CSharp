using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FTIR.Analyzers;
using FTIR.Correctors;

namespace Shokouki.Controllers {
    public abstract class Camera
    {
        protected ConcurrentQueue<ISpectrum> Queue { get; } = new ConcurrentQueue<ISpectrum>();
        public bool IsProcessing { get; protected set; } = false;
        public bool IsOn { get; set; } = false;
        public void Capture(ISpectrum spectrum)
        {
            if (!IsOn)
            {
                throw new Exception("camera is off");
            }
            Queue.Enqueue(spectrum);

            if (IsProcessing) return;
            IsProcessing = true;
            Task.Run(() =>
            {
                ISpectrum dequeue;
                while (Queue.TryDequeue(out dequeue))
                {
                    Consume(dequeue);
                }
                IsProcessing = false;
            });
        }

        protected abstract void Consume(ISpectrum dequeue);
    }
}
