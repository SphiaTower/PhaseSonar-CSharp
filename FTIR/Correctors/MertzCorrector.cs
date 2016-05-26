using System;
using FTIR.Maths;

namespace FTIR.Correctors
{
    public class MertzCorrector : BaseCorrector<RealSpectrum>
    {
        private readonly double[] _centrePhase;
        private readonly int _centreSpan;

        private readonly double[] _interpolatedArray;
        private readonly Interpolator _interpolator;

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
        ///     Do phase correction on a piece of the full temporal data
        /// </summary>
        /// <param name="pulseSequence">the full temporal data</param>
        /// <param name="startIndex">the starting index of the piece</param>
        /// <returns></returns>
        public override void Correct(double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest)
        {
            // Side effect: _zeroFilledArray -> balanced, zero-filled temporal data
            Retrieve(pulseSequence, startIndex, pulseLength);
            // Side effect: _zeroFilledArray -> centreburst rotated to the centre
            try
            {
                Rotator.SymmetrizeInPlace(ZeroFilledArray, pointsBeforeCrest);
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
                WriteBuffer(i, real*Math.Cos(phase) + imag*Math.Sin(phase));
            }
            OnCorrected();
        }

        protected virtual void WriteBuffer(int i, double specPoint)
        {
            SpectrumBuffer.AmplitudeArray[i] = specPoint;
        }

        protected virtual void OnCorrected()
        {
            SpectrumBuffer.PulseCount = 1;
        }


        public void PreparePhaseCorrectionData(double[] symmetryPulse)
        {
            var centerBurst = symmetryPulse.Length/2;
//            Funcs.CopyInto(symmetryPulse, centerBurst - _centreSpan, _centreSpan*2, _centrePhase);
            Array.Copy(symmetryPulse, centerBurst - _centreSpan, _centrePhase, 0, _centreSpan*2);
            Apodizer.Apodize(_centrePhase);
            // Funcs.Rotate<double>(_centrePhase);
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