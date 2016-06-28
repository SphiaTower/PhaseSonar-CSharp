using System;
using System.Collections.Generic;

namespace PhaseSonar.Slicers
{
    /// <summary>
    ///     A concrete implemention of crest finder which finds crests based on the absolute value.
    /// </summary>
    public class AbsoluteCrestFinder : ICrestFinder
    {
        /// <summary>
        ///     Create an absolute crest finder.
        /// </summary>
        /// <param name="repetitionRate">The difference of repetition rate</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="leftThreshold">The minimum number of points that is before the crest.</param>
        /// <param name="verticalThreshold">The minimum absolute amplitude of a crest</param>
        public AbsoluteCrestFinder(double repetitionRate, double sampleRate, int leftThreshold, double verticalThreshold)
        {
            RepetitionRate = repetitionRate;
            SampleRate = sampleRate;
            LeftThreshold = leftThreshold;
            VerticalThreshold = verticalThreshold;
        }

        /// <summary>
        ///     The repetition rate difference.
        /// </summary>
        protected double RepetitionRate { get; }

        /// <summary>
        ///     The sample rate.
        /// </summary>
        protected double SampleRate { get; }

        /// <summary>
        ///     The minimum amplitude of a crest
        /// </summary>
        protected double VerticalThreshold { get; set; }

        /// <summary>
        ///     The minimum number of points that is before the crest.
        /// </summary>
        public int LeftThreshold { get; }

        /// <summary>
        ///     Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The indices of the crests</returns>
        public virtual IList<int> Find(double[] pulseSequence)
        {
            var rightThreshold = SampleRate/(RepetitionRate + 300);

            var maxValue = .0;
            var maxIndex = 0;
            var i = 0;

            var crestIndices = new List<int>();

            foreach (var point in pulseSequence)
            {
                var abs = Math.Abs(point);
                if (abs > maxValue)
                {
                    maxValue = abs;
                    maxIndex = i;
                }
                var distanceAwayFromMax = i - maxIndex;

                if (distanceAwayFromMax > rightThreshold)
                {
                    if (maxValue > VerticalThreshold)
                    {
                        if (maxIndex > LeftThreshold)
                        {
                            crestIndices.Add(maxIndex);
                        }
                    }
                    maxValue = 0;
                    maxIndex = i;
                }
                i++;
            }
            return crestIndices;
        }
    }
}