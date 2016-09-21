using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.CrestFinders {
    /// <summary>
    ///     An improved finder which adjusts the vertical threshold automatically if the number of crests found is not enough.
    /// </summary>
    public class AutoAdjustCrestFinder : ICrestFinder {
        [NotNull] private readonly ICrestFinder _delegateFinder;

        /// <summary>
        ///     Create an instance. <see cref="AbsoluteCrestFinder" />
        /// </summary>
        public AutoAdjustCrestFinder(ICrestFinder delegateFinder) {
            _delegateFinder = delegateFinder;
        }

        /// <summary>
        ///     The minimum number of points that is before the crest.
        /// </summary>
        public int MinPtsCntBeforeCrest => _delegateFinder.MinPtsCntBeforeCrest;

        public double VerticalThreshold {
            get { return _delegateFinder.VerticalThreshold; }
            set { _delegateFinder.VerticalThreshold = value; }
        }

        public double RepetitionRate => _delegateFinder.RepetitionRate;
        public double SampleRate => _delegateFinder.SampleRate;

        /// <summary>
        ///     Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>Whether crests are found successfully</returns>
        public IList<int> Find(double[] pulseSequence) {
            while (_delegateFinder.VerticalThreshold > 0.055) // todo move, now recorded
            {
                var crestIndices = _delegateFinder.Find(pulseSequence);
                var cmp = CompareRepFreq(pulseSequence.Length, crestIndices);
                if (cmp > 0) VerticalThreshold *= 1.2;
                else if (cmp < 0) VerticalThreshold *= 0.9;
                else return crestIndices;
            }
            return _delegateFinder.Find(pulseSequence);
        }

        /// <summary>
        ///     Compare the calculated repetion frequency against the ideal one
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="crests"></param>
        /// <returns></returns>
        protected virtual int CompareRepFreq(int dataLength, [NotNull] IList<int> crests) {
            var temporalLength = dataLength/SampleRate;
            var crestRepFreq = crests.Count/temporalLength;
            // todo comparison threshold
            if (crestRepFreq > RepetitionRate + 250) return 1;
            if (crestRepFreq < RepetitionRate - 250) return -1;
            return 0;
        }
    }
}