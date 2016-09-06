using PhaseSonar.Utils;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer.Writers {
    /// <summary>
    ///     A writer for <see cref="TracedSpectrum" />
    /// </summary>
    public class SpectrumWriter : AbstractDiskWriter<TracedSpectrum> {
        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        public SpectrumWriter(string directory, string prefix, bool on) : base(directory, prefix, on) {
        }

        /// <summary>
        ///     Called when start consuming a dequeued element.
        /// </summary>
        /// <param name="dequeue"></param>
        /// <returns>True if consumed successfully.</returns>
        protected override void ConsumeElement(TracedSpectrum dequeue) {
            Toolbox.WriteStringArray(BasePath + dequeue.Tag.Enclose("No") + dequeue.PulseCount.Enclose("Cnt") + Suffix,
                dequeue.ToStringArray());
        }
    }
}