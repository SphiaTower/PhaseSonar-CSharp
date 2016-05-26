using System;
using FTIR.Maths;

namespace FTIR.Correctors
{
    public class FakeCorrector : BaseCorrector<ComplexSpectrum>
    {

        public FakeCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor)
        {

        }



        public override void Correct(double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest)
        {
            Retrieve(pulseSequence, startIndex, pulseLength);
            try
            {
                Rotator.SymmetrizeInPlace(ZeroFilledArray, pointsBeforeCrest);
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
            // Side effect: _zeroFilledArray -> apodized by a cached ramp
            Apodizer.Apodize(ZeroFilledArray);
            // Side effect: _zeroFilledArray -> centreburst rotated to the head and tail
            Rotator.Rotate(ZeroFilledArray);

            double[] fftReal, fftImag;
            FourierTransformer.TransformForward(ZeroFilledArray, out fftReal, out fftImag);
            SpectrumBuffer.TryAbsorb(new ComplexSpectrum(fftReal,fftImag,1));
        }
     

    }
}