using System;
using System.IO;
using JetBrains.Annotations;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer.Writers {
    public class SpectrumWriterV2 : IWriterV2<TracedSpectrum> {
        protected const string Suffix = ".txt";
        private readonly IWriterV2<TracedSpectrum> _writerV2Implementation;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpectrumWriterV2([NotNull] string directory, [NotNull] string prefix) {
            _writerV2Implementation = new SerialWriterV2<TracedSpectrum>();
            var path = Path.Combine(directory, prefix);
            _writerV2Implementation.ElementDequeued += spectrum => {
                Toolbox.WriteStringArray(
                    path + spectrum.Tag.Enclose("No") + spectrum.PulseCount.Enclose("Cnt") + Suffix,
                    spectrum.ToStringArray());
            };
        }

        /// <summary>
        ///     Enqueue the data to save.
        /// </summary>
        /// <param name="data"></param>
        public void Write(TracedSpectrum data) {
            _writerV2Implementation.Write(data);
        }

        public event Action<TracedSpectrum> ElementDequeued {
            add { _writerV2Implementation.ElementDequeued += value; }
            remove { _writerV2Implementation.ElementDequeued -= value; }
        }
    }
}