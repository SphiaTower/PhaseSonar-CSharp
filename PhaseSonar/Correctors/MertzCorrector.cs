using System;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    /// <summary>
    ///     A phase corrector which implements the Mertz method.
    /// </summary>
    public class MertzCorrector : BaseCorrector<RealSpectrum>
    {
        private readonly double[] _centrePhase;
        private readonly int _centreSpan;

        private readonly double[] _interpolatedArray;
        private readonly Interpolator _interpolator;

        /// <summary>
        ///     Create a mertz corrector.
        /// </summary>
        /// <param name="apodizer">
        ///     <see cref="IApodizer" />
        /// </param>
        /// <param name="fuzzyPulseLength">The approximate period length</param>
        /// <param name="zeroFillFactor">The factor of zero filling</param>
        /// <param name="centreSpan">The size of the centre span for phase extraction</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public MertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor)
        {
            if (centreSpan <= 0)
            {
                throw new ArgumentOutOfRangeException("centreSpan must be positive");
            }
            _centreSpan = centreSpan;
            _interpolatedArray = new double[ZeroFilledLength];
            var centreLength = _centreSpan*2;
            _centrePhase = new double[centreLength];
            _interpolator = new Interpolator(centreLength, ZeroFilledLength);
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
            // Side effect: _zeroFilledArray -> balanced, zero-filled temporal data
            Retrieve(pulseSequence, startIndex, pulseLength);
            // Side effect: _zeroFilledArray -> centreburst rotated to the centre
            try
            {
                Rotator.Symmetrize(ZeroFilledArray, crestIndex);
            }
            catch (Exception)
            {
                return;
            }
            // Side effect: _centrePhase -> phase data of the centre double-sided span
            PreparePhaseCorrectionData(ZeroFilledArray);
            // Side effect: _interpolatedArray -> interpolated phase data
            _interpolator.Interpolate(_centrePhase, _interpolatedArray);

            // Side effect: _zeroFilledArray -> apodized by a cached ramp
            Apodizer.Apodize(ZeroFilledArray);
            // Side effect: _zeroFilledArray -> centreburst rotated to the head and tail
            Rotator.Rotate(ZeroFilledArray);

            double[] fftReal, fftImag;
            FourierTransformer.TransformForward(ZeroFilledArray, out fftReal, out fftImag);

            for (var i = 0; i < ZeroFilledLength/2; i++)
            {
                var phase = _interpolatedArray[i];
                var real = fftReal[i];
                var imag = fftImag[i];
                WriteSpecPoint(i, real*Math.Cos(phase) + imag*Math.Sin(phase));
            }
            OnCorrected();
        }

        /// <summary>
        ///     Write the index and the corresponding spectrum value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="specValue"></param>
        protected virtual void WriteSpecPoint(int i, double specValue)
        {
            SpectrumBuffer.AmplitudeArray[i] = specValue;
        }

        /// <summary>
        ///     Called when the correction is about to finish.
        /// </summary>
        protected virtual void OnCorrected()
        {
            SpectrumBuffer.PulseCount = 1;
        }


        /// <summary>
        ///     Prepare the phase data for correction
        /// </summary>
        /// <param name="symmetryPulse">The symmetrized pulse</param>
        public void PreparePhaseCorrectionData(double[] symmetryPulse)
        {
            var centerBurst = symmetryPulse.Length/2;
//            Functions.CopyInto(symmetryPulse, centerBurst - _centreSpan, _centreSpan*2, _centrePhase);
            Array.Copy(symmetryPulse, centerBurst - _centreSpan, _centrePhase, 0, _centreSpan*2);
            Apodizer.Apodize(_centrePhase);
            // Functions.Rotate<double>(_centrePhase);
            Rotator.Rotate(_centrePhase);
            double[] fftReal, fftImag;
            FourierTransformer.TransformForward(_centrePhase, out fftReal, out fftImag);
            for (var i = 0; i < _centreSpan*2; i++)
            {
                _centrePhase[i] = Math.Atan(fftImag[i]/fftReal[i]);
            }
        }
    }
}