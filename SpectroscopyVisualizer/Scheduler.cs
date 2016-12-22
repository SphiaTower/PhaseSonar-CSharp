using JetBrains.Annotations;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer {
    public interface IScheduler {
        StopWatch Watch { get; }


        /// <summary>
        ///     The producerss
        /// </summary>
        IProducerV2<SampleRecord> Producer { get; }

        /// <summary>
        ///     The consumer
        /// </summary>
        IConsumerV2 Consumer { get; }

        /// <summary>
        ///     Start the system.
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop the system.
        /// </summary>
        void Stop();
    }

    public sealed class EmptyScheduler : IScheduler {
        /// <summary>
        ///     A stopwatch.
        /// </summary>
        public StopWatch Watch { get; } = new StopWatch();

        /// <summary>
        ///     Start the system.
        /// </summary>
        public void Start() {
            Watch.Reset();
        }

        /// <summary>
        ///     Stop the system.
        /// </summary>
        public void Stop() {
        }

        /// <summary>
        ///     The producerss
        /// </summary>
        public IProducerV2<SampleRecord> Producer => null;

        /// <summary>
        ///     The consumer
        /// </summary>
        public IConsumerV2 Consumer => null;
    }

    /// <summary>
    ///     A scheduler scheduling the producer and the consumer.
    /// </summary>
    public sealed class Scheduler : IScheduler {
        /// <summary>
        ///     Create a scheduler.
        /// </summary>
        /// <param name="producer"></param>
        /// <param name="consumer"></param>
        public Scheduler(IProducerV2<SampleRecord> producer, IConsumerV2 consumer) {
            Producer = producer;
            Consumer = consumer;
        }

        /// <summary>
        ///     The producerss
        /// </summary>
        [NotNull]
        public IProducerV2<SampleRecord> Producer { get; }

        /// <summary>
        ///     The consumer
        /// </summary>
        [NotNull]
        public IConsumerV2 Consumer { get; }

        /// <summary>
        ///     A stopwatch.
        /// </summary>
        public StopWatch Watch { get; } = new StopWatch();

        /// <summary>
        ///     Start the system.
        /// </summary>
        public void Start() {
            Watch.Reset();
            Producer.Start();
            Consumer.Start();
        }


        /// <summary>
        ///     Stop the system.
        /// </summary>
        public void Stop() {
            Producer.Stop();

            Consumer.Stop();
        }
    }
}