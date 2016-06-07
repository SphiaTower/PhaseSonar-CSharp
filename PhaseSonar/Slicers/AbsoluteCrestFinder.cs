using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Slicers
{
    /// <summary>
    /// A finder that identifies crests in a pulse sequence.
    /// </summary>
    public interface ICrestFinder
    {
        /// <summary>
        /// The minimum number of points that is before the crest.
        /// </summary>
        int LeftThreshold { get; }
        /// <summary>
        /// Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The start indices of the pulses in the pulse sequence</returns>
        [CanBeNull]
        List<int> Find([NotNull]double[] pulseSequence);
    }


    /// <summary>
    /// A concrete implemention of crest finder which finds crests based on the absolute value.
    /// </summary>
    public class AbsoluteCrestFinder : ICrestFinder
    {
        /// <summary>
        /// Create an absolute crest finder.
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
        /// The repetition rate difference.
        /// </summary>
        protected double RepetitionRate { get; }
        /// <summary>
        /// The sample rate.
        /// </summary>
        protected double SampleRate { get; }
        /// <summary>
        /// The minimum amplitude of a crest
        /// </summary>
        protected double VerticalThreshold { get; set; }

        /// <summary>
        /// The minimum number of points that is before the crest.
        /// </summary>
        public int LeftThreshold { get; }


        /// <summary>
        /// Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The start indices of the pulses in the pulse sequence</returns>
        public virtual List<int> Find(double[] pulseSequence)
        {
            var rightThreshold = SampleRate/(RepetitionRate + 200)/4.0;

            var maxValue = .0;
            var maxIndex = 0;
            var i = 0;

            var peakIndices = new List<int>();

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
                            peakIndices.Add(maxIndex);
                        }
                    }
                    maxValue = 0;
                    maxIndex = i;
                }
                i++;
            }
            return peakIndices.Count==0?null:peakIndices;
        }
    }

    /// <summary>
    /// An improved finder which adjusts the vertical threshold automatically if the number of crests found is not enough.
    /// </summary>
    public class IntelligentAbsoluteCrestFinder : AbsoluteCrestFinder
    {
        /// <summary>
        /// Create an instance. <see cref="AbsoluteCrestFinder"/>
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
        /// Find the crests in a pulse sequence.
        /// </summary>
        /// <param name="pulseSequence">A pulse sequence containing multiple pulses</param>
        /// <returns>The start indices of the pulses in the pulse sequence</returns>
        public override List<int> Find(double[] pulseSequence)
        {
            while (VerticalThreshold > 0.055) // todo move, now recorded
            {
                var crests = base.Find(pulseSequence);
                var cmp = CompareRepFreq(pulseSequence.Length, crests);
                if (cmp > 0) VerticalThreshold = VerticalThreshold*1.2;
                else if (cmp < 0) VerticalThreshold = VerticalThreshold*0.9;
                else return crests;
            }
            return base.Find(pulseSequence);
        }

        /// <summary>
        /// Compare the calculated repetion frequency against the ideal one
        /// </summary>
        /// <param name="dataLength"></param>
        /// <param name="crests"></param>
        /// <returns></returns>
        protected virtual int CompareRepFreq(int dataLength, [CanBeNull]IList<int> crests)
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