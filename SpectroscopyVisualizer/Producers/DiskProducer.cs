using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     A producer which retrieves data from files on disk.
    /// </summary>
    public class DiskProducer : AbstractProducer<SampleRecord>
    {
        private readonly IEnumerator<string> _enumerator;
        private readonly Regex _regex = new Regex("\\[No-(.+)\\]");


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
                var path = _enumerator.Current;
                var match = _regex.Match(path);
                int num;
                if (!int.TryParse(match.Value,out num))
                {
                    num = ProductCnt;
                }
                return new SampleRecord(Toolbox.DeserializeData<double[]>(path), num);
            }
            IsOn = false;
            return null;
        }
    }
}