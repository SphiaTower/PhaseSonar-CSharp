using System;
using System.Collections.Generic;
using PhaseSonar.Maths;
using PhaseSonar.Utils;
using JetBrains.Annotations;
using MathNet.Numerics.Transformations;

namespace PhaseSonar.Correctors
{
    public abstract class BaseCorrector<T> : ICorrector<T> where T : ISpectrum
    {
        protected BaseCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor)
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
            Apodizer = apodizer;

            ZeroFilledLength = CalZeroFilledLength(fuzzyPulseLength, zeroFillFactor);
            ZeroFilledArray = new double[ZeroFilledLength];

            OutputLength = ZeroFilledLength/2;
            Rotator = new Rotator();
            FourierTransformer = new RealFourierTransformation();

            SpectrumBuffer = SpectrumFactory<T>.CreateEmptySpectrum(OutputLength);
        }
        protected T SpectrumBuffer { get; }

        protected IApodizer Apodizer { get; }
        protected RealFourierTransformation FourierTransformer { get; }
        protected Rotator Rotator { get; }
        protected double[] ZeroFilledArray { get; }
        protected int ZeroFilledLength { get; }


        public T OutputSpetrumBuffer()
        {
            return SpectrumBuffer;
        }
        public int OutputLength { get; }

        /// <summary>
        ///     Do phase correction on a piece of the full temporal data
        /// </summary>
        /// <param name="pulseSequence">the full temporal data</param>
        /// <param name="startIndex">the starting index of the piece</param>
        /// <returns></returns>
        public abstract void Correct(double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest);

        public void ClearBuffer()
        {
            SpectrumBuffer.Clear();
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

        protected static double RangeSum(IReadOnlyList<double> interferogram, int offset, int length)
        {
            var sum = .0;
            for (var i = offset; i < offset + length; i++)
            {
                sum += interferogram[i];
            }
            return sum;
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
    }
}