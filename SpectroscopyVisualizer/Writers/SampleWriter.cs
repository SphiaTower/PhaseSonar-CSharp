using System;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    ///     A Writer writing data sampled.
    /// </summary>
    public class SampleWriter : AbstractDiskWriter<SampleRecord>
    {
        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        public SampleWriter(string directory, string prefix, bool on) : base(directory, prefix, on)
        {
        }

        /// <summary>
        ///     Called when start consuming a dequeued element.
        /// </summary>
        /// <param name="dequeue"></param>
        /// <returns>True if consumed successfully.</returns>
        protected override void ConsumeElement(SampleRecord dequeue) // todo
        {
            try
            {
                Toolbox.SerializeData(BasePath +  "-" + dequeue.ID + Suffix, dequeue.PulseSequence);
            }
            catch (Exception)
            {
                // todo
            }
        }
    }
}