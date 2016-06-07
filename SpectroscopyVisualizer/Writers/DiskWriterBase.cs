using System;
using System.IO;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    /// A base implementation of <see cref="Writer{T}"/>, used to save data into local disks.
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public abstract class DiskWriterBase<T> : Writer<T>
    {
        /// <summary>
        /// The suffix of the file for data to save.
        /// </summary>
        protected const string Suffix = ".txt";

        private int _fileIndex;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        protected DiskWriterBase(string directory, string prefix, bool on) : base(on)
        {
            BasePath = Path.Combine(directory, prefix);
        }

        /// <summary>
        /// The time-stamp of the data saved.
        /// </summary>
        protected string TimeStamp { get; } = DateTime.Now.ToShortTimeString().Remove(2, 1);

        /// <summary>
        /// The base path containing the directory and the prefix.
        /// </summary>
        protected string BasePath { get; }

        /// <summary>
        /// Save data element in the queue.
        /// </summary>
        /// <param name="dequeue">The dequeued element</param>
        protected override void ConsumeElement(T dequeue)
        {
            /*   if (_fileIndex == -1)
            {
                _fileIndex = GetMaxIndex(_directory) + 1; // todo buggy
            }*/
            if (WriteData(dequeue, BasePath, _fileIndex)) // todo badly designed callback
            {
                _fileIndex++;
            }
        }

        /// <summary>
        /// Called when start consuming a dequeued element.
        /// </summary>
        /// <param name="dequeue"></param>
        /// <param name="basePath"></param>
        /// <param name="fileIndex"></param>
        /// <returns>True if consumed successfully.</returns>
        protected abstract bool WriteData(T dequeue, string basePath, int fileIndex);
    }
}