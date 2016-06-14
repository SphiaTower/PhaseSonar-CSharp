using PhaseSonar.Correctors;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Writers
{
    public class SpectrumWriter : AbstractDiskWriter<ISpectrum>
    {
        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        public SpectrumWriter(string directory, string prefix, bool on) : base(directory, prefix, on)
        {
        }

        /// <summary>
        ///     Called when start consuming a dequeued element.
        /// </summary>
        /// <param name="dequeue"></param>
        /// <param name="basePath"></param>
        /// <param name="fileIndex"></param>
        /// <returns>True if consumed successfully.</returns>
        protected override bool WriteData(ISpectrum dequeue, string basePath, int fileIndex)
        {
            Toolbox.WriteStringArray(basePath + "t" + TimeStamp + "-" + fileIndex + "-n" + dequeue.PulseCount + Suffix,
                dequeue.ToStringArray());
            return true;
        }
    }
}