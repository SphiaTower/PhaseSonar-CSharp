using System;
using System.Diagnostics;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    public class AccMertzCorrector : MertzCorrector
    {
        public AccMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
        }


        protected override void OnCorrected()
        {
            SpectrumBuffer.PulseCount++;
        }

        protected override void WriteBuffer(int i, double specPoint)
        {
            SpectrumBuffer.AmplitudeArray[i] += specPoint;
        }
    }
}