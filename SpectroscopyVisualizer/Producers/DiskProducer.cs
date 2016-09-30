using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Producers {
    /// <summary>
    ///     A producer which retrieves data from files on disk.
    /// </summary>
    public class DiskProducer : AbstractProducer<SampleRecord> {
        private readonly IEnumerator<string> _enumerator;

        [NotNull] private readonly Func<string, double[]> _funcRead;

        private readonly Regex _regex = new Regex("\\[No-(.+)\\]");

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="paths">The paths of files to be processed.</param>
        public DiskProducer(IEnumerable<string> paths, bool compressed) : base(48) // todo
        {
            _enumerator = paths.GetEnumerator();
            if (compressed) {
                _funcRead = Toolbox.DeserializeData<double[]>;
            } else {
                _funcRead = Toolbox.ReadMemoryEfficiently;
            }
        }

        /// <summary>
        ///     A callback called before retrieving data in this turn.
        /// </summary>
        protected override void OnPreRetrieve() {
        }

        /// <summary>
        ///     Retrieve data into the blocking queue.
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        protected override SampleRecord RetrieveData() {
            if (_enumerator.MoveNext()) {
                var path = _enumerator.Current;
                var match = _regex.Match(path);
                int num;
                if (!int.TryParse(match.Value, out num)) {
                    num = ProductCnt;
                }
                return CreateRecord(path, num);
            }
            IsOn = false;
            return null;
        }

        [NotNull]
        protected virtual SampleRecord CreateRecord(string path, int num) {
            return new SampleRecord(_funcRead(path),num);
            
        }
    }
}