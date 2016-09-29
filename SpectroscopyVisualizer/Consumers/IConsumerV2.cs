using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Consumers {
    public interface IConsumerV2 {
        /// <summary>
        ///     The number of elements have been consumed.
        /// </summary>
        int ConsumedCnt { get; }

        int TargetCnt { get; }
        /// <summary>
        ///     Stop consuming.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Start consuming.
        /// </summary>
        void Start();

        event Action SourceInvalid;

        event Action ElementConsumedSuccessfully;

        event Action ProducerEmpty;

        event Action TargetAmountReached;
    }
}
