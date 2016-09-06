using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Consumers {
    public class DataSerializer : ParallelConsumer<SampleRecord, SpecialSampleWriter> {
        /// <summary>
        ///     Create a Consumer.
        /// </summary>
        /// <param name="blockingQueue">The queue which containing elements to be consumed</param>
        /// <param name="workers">A collection of workers consuming the products in parallel.</param>
        public DataSerializer([NotNull] BlockingCollection<SampleRecord> blockingQueue,
            [NotNull] IEnumerable<SpecialSampleWriter> workers) : base(blockingQueue, workers) {
        }

        /// <summary>
        ///     Consume element in a branch.
        /// </summary>
        /// <param name="element">The product</param>
        /// <param name="worker">The worker in this branch.</param>
        /// <returns>Whether consuming succeeds.</returns>
        protected override bool ConsumeElement(SampleRecord element, SpecialSampleWriter worker) {
            worker.Write(element);
            return true;
        }
    }

    public class SpecialSampleWriter {
        protected const string Suffix = ".txt";

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpecialSampleWriter(string directory, string prefix) {
            BasePath = Path.Combine(directory, prefix);
        }

        public string BasePath { get; }

        public void Write(SampleRecord record) {
            Toolbox.SerializeData(BasePath + record.Id.Enclose("No") + Suffix, record.PulseSequence);
        }
    }
}