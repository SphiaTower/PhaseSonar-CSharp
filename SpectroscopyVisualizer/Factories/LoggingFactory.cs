using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Slicers;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Utilities;

namespace SpectroscopyVisualizer.Factories {
    public class LoggingFactory : ParallelInjector {
        [NotNull]
        public override ISlicer NewSlicer() {
            return new LoggingSlicer(base.NewSlicer());
        }

        [NotNull]
        public override ICrestFinder NewCrestFinder() {
            return new LoggingCrestFinder(base.NewCrestFinder());
        }

        private class LoggingSlicer : ISlicer {
            private readonly ISlicer _delegate;

            /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
            internal LoggingSlicer(ISlicer @delegate) {
                _delegate = @delegate;
            }

            /// <summary>
            ///     Slice the pulse sequence.
            /// </summary>
            /// <param name="pulseSequence">A pulse sequence, usually a sampled record</param>
            /// <returns>Start indices of pulses of different components, for example, gas and reference</returns>
            [NotNull]
            public List<SliceInfo> Slice(double[] pulseSequence, IList<int> crestIndices) {
                var sliceInfos = _delegate.Slice(pulseSequence, crestIndices);
                Logger.WriteLine("切片", "得到" + sliceInfos.Count + "个有效周期");
                return sliceInfos;
            }
        }

        private class LoggingCrestFinder : ICrestFinder {
            private readonly ICrestFinder _finder;

            public LoggingCrestFinder(ICrestFinder finder) {
                _finder = finder;
            }

            /// <summary>
            ///     The minimum number of points that is before the crest.
            /// </summary>
            public int MinPtsCntBeforeCrest => _finder.MinPtsCntBeforeCrest;

            public double VerticalThreshold {
                get { return _finder.VerticalThreshold; }
                set { _finder.VerticalThreshold = value; }
            }

            public double RepetitionRate => _finder.RepetitionRate;

            public double SampleRate => _finder.SampleRate;

            /// <summary>
            ///     Find the crests in a pulse sequence.
            /// </summary>
            /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
            /// <returns>The indices of the crests</returns>
            public IList<int> Find(double[] pulseSequence) {
                var crestIndices = _finder.Find(pulseSequence);
                Logger.WriteLine("找峰", "找到" + crestIndices.Count + "个峰");
                return crestIndices;
            }
        }
    }
}