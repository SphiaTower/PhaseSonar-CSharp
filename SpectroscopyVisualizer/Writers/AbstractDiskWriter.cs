using System;
using System.IO;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    ///     A base implementation of <see cref="AbstractWriter{T}" />, used to save data into local disks.
    /// </summary>
    /// <typeparam name="T">The type of data</typeparam>
    public abstract class AbstractDiskWriter<T> : AbstractWriter<T>
    {
        /// <summary>
        ///     The suffix of the file for data to save.
        /// </summary>
        protected const string Suffix = ".txt";


        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        protected AbstractDiskWriter(string directory, string prefix, bool on) : base(on)
        {
            BasePath = Path.Combine(directory, prefix);
        }
        /// <summary>
        ///     The time-stamp of the data saved.
        /// </summary>
        protected string TimeStamp { get; } = DateTime.Now.ToShortTimeString().Remove(2, 1);

        /// <summary>
        ///     The base path containing the directory and the prefix.
        /// </summary>
        protected string BasePath { get; }

    }
}