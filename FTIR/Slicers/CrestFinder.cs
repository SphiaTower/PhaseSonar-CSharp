using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FTIR.Slicers
{
    public interface ICrestFinder
    {
        int LeftThreshold { get; }
        [CanBeNull]
        List<int> Find([NotNull]double[] pulseSequence);
    }

    public class NoCrestFoundException : Exception
    {
    }

    public class CrestFinder : ICrestFinder
    {
        public CrestFinder(double repetitionRate, double samplingRate, int leftThreshold, double verticalThreshold)
        {
            RepetitionRate = repetitionRate;
            SamplingRate = samplingRate;
            LeftThreshold = leftThreshold;
            VerticalThreshold = verticalThreshold;
        }

        protected double RepetitionRate { get; }
        protected double SamplingRate { get; }
        protected double VerticalThreshold { get; set; }
        public int LeftThreshold { get; }


        public virtual List<int> Find(double[] pulseSequence)
        {
            var rightThreshold = SamplingRate/(RepetitionRate + 200)/4.0;

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

    public class IntelligentCrestFinder : CrestFinder
    {
        public IntelligentCrestFinder(double repetitionRate, double samplingRate, int leftThreshold,
            double verticalThreshold) : base(repetitionRate, samplingRate, leftThreshold, verticalThreshold)
        {
        }

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

        protected virtual int CompareRepFreq(int dataLength, [CanBeNull]IList<int> crests)
        {
            var temporalLength = dataLength/SamplingRate;
            var count = crests?.Count ?? 0;
            var crestRepFreq = count/temporalLength;
            // todo comparison threshold
            if (crestRepFreq > RepetitionRate + 250) return 1;
            if (crestRepFreq < RepetitionRate - 250) return -1;
            return 0;
        }
    }
}