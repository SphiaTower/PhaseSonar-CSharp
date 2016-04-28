using System;
using System.Collections.Generic;
using FTIR.Maths;
using FTIR.Utils;
using MathNet.Numerics.Transformations;

namespace FTIR.Correctors
{
    public class MertzCorrector : ICorrector
    {
        private readonly IApodizer _apodizer;

        private readonly double[] _centrePhase;
        private readonly int _centreSpan;
        private readonly RealFourierTransformation _fourierTransformer;

        private readonly double[] _interpolatedArray;
        private readonly Interpolator _interpolator;
        private readonly Rotator _rotator;

        public MertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan = 256)
        {
            Toolbox.RequireNonNull(apodizer);
            if (fuzzyPulseLength <= 0)
            {
                throw new ArgumentOutOfRangeException("fuzzyPulseLength must be positive");
            }
            if (zeroFillFactor <= 0)
            {
                throw new ArgumentOutOfRangeException("zero fill factor must >=1");
            }
            if (centreSpan <= 0)
            {
                throw new ArgumentOutOfRangeException("centreSpan must be positive");
            }
            _apodizer = apodizer;
            _centreSpan = centreSpan;

            ZeroFilledLength = CalZeroFilledLength(fuzzyPulseLength, zeroFillFactor);
            _interpolatedArray = new double[ZeroFilledLength];
            ZeroFilledArray = new double[ZeroFilledLength];

            Output = new double[ZeroFilledLength/2];
            var centreLength = _centreSpan*2;
            _centrePhase = new double[centreLength];
            _interpolator = new Interpolator(centreLength, ZeroFilledLength);

            _rotator = new Rotator();
            _fourierTransformer = new RealFourierTransformation();
        }

        public double[] ZeroFilledArray { get; }
        protected int ZeroFilledLength { get; }


        public int OutputLength => ZeroFilledLength/2;

        public double[] Output { get; }

        public virtual int OutputPeriodCnt()
        {
            return 1;
        }

        public virtual void ClearBuffer()
        {
            for (var i = 0; i < Output.Length; i++)
            {
                Output[i] = 0;
            }
        }

        /// <summary>
        ///     Do phase correction on a piece of the full temporal data
        /// </summary>
        /// <param name="pulseSequence">the full temporal data</param>
        /// <param name="startIndex">the starting index of the piece</param>
        /// <returns></returns>
        public virtual void Correct(double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest)
        {
            // Side effect: _zeroFilledArray -> balanced, zero-filled temporal data
            Retrieve(pulseSequence, startIndex, pulseLength);
            // Side effect: _zeroFilledArray -> centreburst rotated to the centre
            _rotator.SymmetrizeInPlace(ZeroFilledArray, pointsBeforeCrest);
            // Side effect: _centrePhase -> phase data of the centre double-sided span
            PreparePhaseCorrectionData(ZeroFilledArray);
            // Side effect: _interpolatedArray -> interpolated phase data
            _interpolator.Interpolate(_centrePhase, _interpolatedArray);

            // Side effect: _zeroFilledArray -> apodized by a cached ramp
            _apodizer.Apodize(ZeroFilledArray);
            // Side effect: _zeroFilledArray -> centreburst rotated to the head and tail
            _rotator.Rotate(ZeroFilledArray);

            double[] fftReal, fftImag;
            _fourierTransformer.TransformForward(ZeroFilledArray, out fftReal, out fftImag);

            for (var i = 0; i < ZeroFilledLength/2; i++)
            {
                var phase = _interpolatedArray[i];
                var real = fftReal[i];
                var imag = fftImag[i];
                WriteBuffer(i, real*Math.Cos(phase) + imag*Math.Sin(phase));
            }
        }


        protected virtual void WriteBuffer(int i, double specPoint)
        {
            Output[i] = specPoint;
        }


        public static int CalZeroFilledLength(int dataLength, int zeroFillFactor)
        {
            return (int) Math.Pow(2, (int) Math.Log(dataLength, 2) + zeroFillFactor);
        }


        public static void Balance(double[] array)
        {
            Balance(array, 0, array.Length);
        }

        public static void Balance(double[] array, int offset, int length)
        {
            var sum = RangeSum(array, offset, length);
            var mean = sum/length;
            for (var i = offset; i < offset + length; i++)
            {
                array[i] = array[i] - mean;
            }
        }

        protected virtual void Retrieve(IReadOnlyList<double> pulseSequence, int startIndex, int pulseLength)
        {
            if (pulseLength > ZeroFilledLength)
            {
                pulseLength = ZeroFilledLength;
                //todo warning: buffer size too small
            }
            var sum = RangeSum(pulseSequence, startIndex, pulseLength);
            var mean = sum/pulseLength;
            for (var i = 0; i < pulseLength; i++,startIndex++)
            {
                ZeroFilledArray[i] = pulseSequence[startIndex] - mean;
            }
            for (var i = pulseLength; i < ZeroFilledLength; i++)
            {
                ZeroFilledArray[i] = 0;
            }
        }

        protected static double RangeSum(IReadOnlyList<double> interferogram, int offset, int length)
        {
            var sum = .0;
            for (var i = offset; i < offset + length; i++)
            {
                sum += interferogram[i];
            }
            return sum;
        }

        public void PreparePhaseCorrectionData(double[] symmetryPulse)
        {
            var centerBurst = symmetryPulse.Length/2;
//            Funcs.CopyInto(symmetryPulse, centerBurst - _centreSpan, _centreSpan*2, _centrePhase);
            Array.Copy(symmetryPulse, centerBurst - _centreSpan, _centrePhase, 0, _centreSpan*2);
            _apodizer.Apodize(_centrePhase);
            // Funcs.Rotate<double>(_centrePhase);
            _rotator.Rotate(_centrePhase);
            double[] fftReal, fftImag;
            _fourierTransformer.TransformForward(_centrePhase, out fftReal, out fftImag);
            for (var i = 0; i < _centreSpan*2; i++)
            {
                _centrePhase[i] = Math.Atan(fftImag[i]/fftReal[i]);
            }
        }
    }
}