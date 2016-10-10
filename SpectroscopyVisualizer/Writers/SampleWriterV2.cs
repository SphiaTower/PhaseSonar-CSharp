using System;
using System.IO;
using JetBrains.Annotations;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer.Writers {
    public class SampleWriterV2 : IWriterV2<SampleRecord> {
        protected const string Suffix = ".txt";
        private readonly IWriterV2<SampleRecord> _writer;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SampleWriterV2([NotNull] string directory, [NotNull] string prefix) {
            _writer = new SerialWriterV2<SampleRecord>();
            var path = Path.Combine(directory, prefix);
            _writer.ElementDequeued += record => {
                try {
                    Toolbox.SerializeData(path + record.Id.Enclose("No") + Suffix, record.PulseSequence);
                } catch (Exception) {
                }
            };
        }

        /// <summary>
        ///     Enqueue the data to save.
        /// </summary>
        /// <param name="data"></param>
        public void Write(SampleRecord data) {
            _writer.Write(data);
        }

        public event Action<SampleRecord> ElementDequeued {
            add { _writer.ElementDequeued += value; }
            remove { _writer.ElementDequeued -= value; }
        }
    }
}