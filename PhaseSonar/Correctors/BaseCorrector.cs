using System;
using System.Collections.Generic;
using PhaseSonar.Maths;
using PhaseSonar.Utils;
using JetBrains.Annotations;
using MathNet.Numerics.Transformations;

namespace PhaseSonar.Correctors
{
    /// <summary>
    /// The base structure of a corrector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseCorrector<T> : ICorrector<T> where T : ISpectrum
    {
        /// <summary>
        /// Create an prototype. <see cref="ICorrector{T}"/>
        /// </summary>
        /// <param name="apodizer"></param>
        /// <param name="fuzzyPulseLength"></param>
        /// <param name="zeroFillFactor"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
        /// <summary>
        /// The buffer for output.
        /// </summary>
        protected T SpectrumBuffer { get; }

        /// <summary>
        /// <see cref="IApodizer"/>
        /// </summary>
        protected IApodizer Apodizer { get; }
        /// <summary>
        /// A transformer which performs FFT.
        /// </summary>
        protected RealFourierTransformation FourierTransformer { get; }
        /// <summary>
        /// <see cref="PhaseSonar.Maths.Rotator"/>
        /// </summary>
        protected Rotator Rotator { get; }
        /// <summary>
        /// An auauxiliaryx container for data of zero-filled size.
        /// </summary>
        protected double[] ZeroFilledArray { get; }
        /// <summary>
        /// The data length after zero filling.
        /// </summary>
        protected int ZeroFilledLength { get; }


        /// <summary>
        /// Get the buffer which stores the latest output
        /// </summary>
        /// <returns>The buffer which stores the latest output</returns>
        public T OutputSpetrumBuffer() {
            return SpectrumBuffer;
        }

        /// <summary>
        /// The length of the output
        /// </summary>
        public int OutputLength { get; }


        /// <summary>
        /// Correct a pulse
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence that the pulse contains in</param>
        /// <param name="startIndex">The start index of the pulse in the pulse sequence</param>
        /// <param name="pulseLength">The length of the pulse</param>
        /// <param name="crestIndex">The number of points before the crest</param>
        public abstract void Correct(double[] pulseSequence, int startIndex, int pulseLength, int crestIndex);

        /// <summary>
        /// Reset the status of the corrector
        /// </summary>
        public void ClearBuffer()
        {
            SpectrumBuffer.Clear();
        }


        /// <summary>
        /// Calculate the length after zero filling.
        /// </summary>
        /// <param name="dataLength">The length of data</param>
        /// <param name="zeroFillFactor">The zero fill factor</param>
        /// <returns></returns>
        public static int CalZeroFilledLength(int dataLength, int zeroFillFactor)
        {
            return (int) Math.Pow(2, (int) Math.Log(dataLength, 2) + zeroFillFactor);
        }

        /// <summary>
        /// Remove the base line of the array
        /// </summary>
        /// <param name="array"></param>
        public static void Balance(double[] array)
        {
            Balance(array, 0, array.Length);
        }

        /// <summary>
        /// Remove the base line of the array
        /// </summary>
        public static void Balance(double[] array, int offset, int length)
        {
            var sum = RangeSum(array, offset, length);
            var mean = sum/length;
            for (var i = offset; i < offset + length; i++)
            {
                array[i] = array[i] - mean;
            }
        }

        /// <summary>
        /// Calculate the sum of a specified range
        /// </summary>
        /// <param name="array">The input array</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length of the range.</param>
        /// <returns></returns>
        protected static double RangeSum(IReadOnlyList<double> array, int startIndex, int length)
        {
            var sum = .0;
            for (var i = startIndex; i < startIndex + length; i++)
            {
                sum += array[i];
            }
            return sum;
        }

        /// <summary>
        /// Retrive the pulse to be corrected in this run, and preprocess it.
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence that contains the pulse to be processed</param>
        /// <param name="startIndex">The start index of the pulse to be processed</param>
        /// <param name="pulseLength">The length of the pulse</param>
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