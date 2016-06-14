using System;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    ///     A Writer writing data sampled.
    /// </summary>
    public class SampleWriter : AbstractDiskWriter<double[]>
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
        /// <param name="basePath"></param>
        /// <param name="fileIndex"></param>
        /// <returns>True if consumed successfully.</returns>
        protected override bool WriteData(double[] dequeue, string basePath, int fileIndex)
        {
            try
            {
                Toolbox.SerializeData(basePath + TimeStamp + "-" + fileIndex + Suffix, dequeue);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}