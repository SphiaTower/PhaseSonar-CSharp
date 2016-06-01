using System;
using System.Diagnostics;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    /// <summary>
    /// A modified mertz corrector which keep on accumulating results until cleared.
    /// </summary>
    public class AccMertzCorrector : MertzCorrector
    {
        /// <summary>
        /// Create an instance. <see cref="ICorrector{T}"/>
        /// </summary>
        /// <param name="apodizer"></param>
        /// <param name="fuzzyPulseLength"></param>
        /// <param name="zeroFillFactor"></param>
        /// <param name="centreSpan"></param>
        public AccMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
        }


        /// <summary>
        ///     Called when the correction is about to finish.
        /// </summary>
        protected override void OnCorrected()
        {
            SpectrumBuffer.PulseCount++;
        }

        /// <summary>
        ///     Write the index and the corresponding spectrum value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="specValue"></param>
        protected override void WriteSpecPoint(int i, double specValue)
        {
            SpectrumBuffer.AmplitudeArray[i] += specValue;
        }
    }
}