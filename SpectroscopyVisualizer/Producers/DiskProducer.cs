using System.Collections.Generic;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     A producer which retrieves data from files on disk.
    /// </summary>
    public class DiskProducer : AbstractProducer<SampleRecord>
    {
        private readonly IEnumerator<string> _enumerator;


        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="paths">The paths of files to be processed.</param>
        public DiskProducer(IEnumerable<string> paths)
        {
            _enumerator = paths.GetEnumerator();
        }

        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected override void OnPreRetrieve()
        {
        }

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        protected override SampleRecord RetrieveData()
        {
            if (_enumerator.MoveNext())
            {
                return new SampleRecord(Toolbox.Read(_enumerator.Current), HistoryProductCnt);
            }
            IsOn = false;
            return null;
        }
    }
}