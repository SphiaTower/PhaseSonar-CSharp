using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
