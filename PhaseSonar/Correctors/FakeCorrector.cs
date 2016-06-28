using System;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    /// <summary>
    ///     A fake corrector which just balances and symmetrizes the data and does no phase correction
    /// </summary>
    public class FakeCorrector : BaseCorrector<ComplexSpectrum>
    {
        /// <summary>
        ///     Create a fake corrector.
        /// </summary>
        /// <param name="apodizer">
        ///     <see cref="BaseCorrector{T}" />
        /// </param>
        /// <param name="fuzzyPulseLength">
        ///     <see cref="BaseCorrector{T}" />
        /// </param>
        /// <param name="zeroFillFactor">
        ///     <see cref="BaseCorrector{T}" />
        /// </param>
        public FakeCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor)
        {
        }


        /// <summary>
        ///     Correct a pulse
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence that the pulse contains in</param>
        /// <param name="startIndex">The start index of the pulse in the pulse sequence</param>
        /// <param name="pulseLength">The length of the pulse</param>
        /// <param name="crestIndex">The number of points before the crest</param>
        public override void Correct(double[] pulseSequence, int startIndex, int pulseLength, int crestIndex)
        {
            Retrieve(pulseSequence, startIndex, pulseLength);
                if(!Rotator.TrySymmetrize(ZeroFilledArray, crestIndex)) return;
            
            // Side effect: _zeroFilledArray -> apodized by a cached ramp
            Apodizer.Apodize(ZeroFilledArray);
            // Side effect: _zeroFilledArray -> centreburst rotated to the head and tail
            Rotator.Rotate(ZeroFilledArray);

            double[] fftReal, fftImag;
            FourierTransformer.TransformForward(ZeroFilledArray, out fftReal, out fftImag);
            // length of fftReal is twice of spectrum buffer
            SpectrumBuffer.TryAbsorb(new ComplexSpectrum(fftReal, fftImag, 1));
        }
    }
}