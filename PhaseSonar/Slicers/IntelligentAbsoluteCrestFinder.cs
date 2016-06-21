using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers
{
    /// <summary>
    ///     An improved finder which adjusts the vertical threshold automatically if the number of crests found is not enough.
    /// </summary>
    public class IntelligentAbsoluteCrestFinder : AbsoluteCrestFinder
    {
        /// <summary>
        ///     Create an instance. <see cref="AbsoluteCrestFinder" />
        /// </summary>
        /// <param name="repetitionRate"></param>
        /// <param name="sampleRate"></param>
        /// <param name="leftThreshold"></param>
        /// <param name="verticalThreshold"></param>
        public IntelligentAbsoluteCrestFinder(double repetitionRate, double sampleRate, int leftThreshold,
            double verticalThreshold) : base(repetitionRate, sampleRate, leftThreshold, verticalThreshold)
        {
        }

        /// <summary>
        ///     Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <param name="crestIndices">The indices of crests</param>
        /// <returns>Whether crests are found successfully</returns>
        public override bool Find(double[] pulseSequence, out IList<int> crestIndices)
        {
            while (VerticalThreshold > 0.055) // todo move, now recorded
            {
                if (base.Find(pulseSequence, out crestIndices))
                {
                    var cmp = CompareRepFreq(pulseSequence.Length, crestIndices);
                    if (cmp > 0) VerticalThreshold *=1.2;
                    else if (cmp < 0) VerticalThreshold *=0.9;
                    else return true;
                }
                else
                {
                    VerticalThreshold *= 0.9;
                }
            }
            return base.Find(pulseSequence, out crestIndices);
        }

        /// <summary>
        ///     Compare the calculated repetion frequency against the ideal one
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="crests"></param>
        /// <returns></returns>
        protected virtual int CompareRepFreq(int dataLength, [CanBeNull] IList<int> crests)
        {
            var temporalLength = dataLength/SampleRate;
            var count = crests?.Count ?? 0;
            var crestRepFreq = count/temporalLength;
            // todo comparison threshold
            if (crestRepFreq > RepetitionRate + 250) return 1;
            if (crestRepFreq < RepetitionRate - 250) return -1;
            return 0;
        }
    }
}