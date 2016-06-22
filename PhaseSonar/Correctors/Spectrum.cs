using System;
using JetBrains.Annotations;
using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    /// <summary>
    ///     The data container for spectra, which contains not only the spectrum data, also the count of accumulation.
    /// </summary>
    public interface ISpectrum
    {
        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        int PulseCount { get; set; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        ISpectrum Clone();

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        bool TryAbsorb([NotNull] ISpectrum another);

        /// <summary>
        ///     Get the intensity of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Intensity(int index);

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Real(int index);

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double Imag(int index);

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        bool HasImag();

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        double AverageIntensity(int index);

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        int Length();
    }

    /// <summary>
    ///     A generic factory which creates spectrum containers of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the spectrum</typeparam>
    public class SpectrumFactory<T> where T : ISpectrum
    {
        /// <summary>
        ///     Create empty spectrum containers of the specified type.
        /// </summary>
        /// <param name="size">The size of the container</param>
        /// <returns>An empty ISpectrum instance</returns>
        public static T CreateEmptySpectrum(int size)
        {
            return (T) Activator.CreateInstance(typeof(T), size);
        }
    }

    /// <summary>
    ///     A spectrum that contains only real numbers
    /// </summary>
    public class RealSpectrum : ISpectrum
    {
        /// <summary>
        ///     Create a real spectrum instance
        /// </summary>
        /// <param name="spectrum">The data of the spectrum</param>
        /// <param name="pulseCount">The number of pulses accumulated</param>
        public RealSpectrum(double[] spectrum, int pulseCount)
        {
            AmplitudeArray = spectrum;
            PulseCount = pulseCount;
        }

        /// <summary>
        ///     Create an empty container
        /// </summary>
        /// <param name="size">The size of the container</param>
        public RealSpectrum(int size)
        {
            AmplitudeArray = new double[size];
            PulseCount = 0;
        }

        /// <summary>
        ///     The amplitude data
        /// </summary>
        public double[] AmplitudeArray { get; }

        /// <summary>
        ///     A unique id used to verify the source of the spectrum.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     The number of pulses accumulated
        /// </summary>
        public int PulseCount { get; set; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        public void Clear()
        {
            Functions.Clear(AmplitudeArray);
            PulseCount = 0;
        }

        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        public ISpectrum Clone()
        {
            return new RealSpectrum(Functions.Clone(AmplitudeArray), PulseCount);
        }

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another)
        {
            if (Length() != another.Length() || another.HasImag())
            {
                return false;
            }
            for (var i = 0; i < AmplitudeArray.Length; i++)
            {
                AmplitudeArray[i] += another.Real(i);
            }
            PulseCount += another.PulseCount;
            return true;
        }

        /// <summary>
        ///     Get the intensity of the data at a specified index
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index)
        {
            return AmplitudeArray[index]*AmplitudeArray[index];
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index)
        {
            return AmplitudeArray[index];
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index)
        {
            return 0;
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag()
        {
            return false;
        }

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double AverageIntensity(int index)
        {
            return Intensity(index)/(PulseCount*PulseCount);
        }

        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length()
        {
            return AmplitudeArray.Length;
        }

        /// <summary>
        ///     Get the string representation of the data at a specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string ToString(int index)
        {
            return AmplitudeArray[index].ToString();
        }

        /// <summary>
        ///     Get the string representation of the whole data array.
        /// </summary>
        /// <returns></returns>
        public string[] ToStringArray()
        {
            var array = new string[Length()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ToString(i);
            }
            return array;
        }
    }

    /// <summary>
    ///     A spectrum that contains only complex numbers
    /// </summary>
    public class ComplexSpectrum : ISpectrum
    {
        /// <summary>
        ///     Create a complex spectrum instance
        /// </summary>
        /// <param name="real">The real part of the spectrum</param>
        /// <param name="imag">The imag part of the spectrum</param>
        /// <param name="pulseCount">The number of pulses accumulated</param>
        public ComplexSpectrum(double[] real, double[] imag, int pulseCount)
        {
            RealArray = real;
            ImagArray = imag;
            PulseCount = pulseCount;
        }

        /// <summary>
        ///     Create an empty container
        /// </summary>
        /// <param name="size">The size of the container</param>
        public ComplexSpectrum(int size)
        {
            RealArray = new double[size];
            ImagArray = new double[size];
            PulseCount = 0;
        }

        /// <summary>
        ///     The real part.
        /// </summary>
        public double[] RealArray { get; }

        /// <summary>
        ///     The imag part
        /// </summary>
        public double[] ImagArray { get; }

        /// <summary>
        ///     A unique id used to verify the source of the spectrum.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     The number of pulses that are accumulated in this container.
        /// </summary>
        public int PulseCount { get; set; }

        /// <summary>
        ///     Clear the container.
        /// </summary>
        public void Clear()
        {
            Functions.Clear(RealArray);
            Functions.Clear(ImagArray);
            PulseCount = 0;
        }


        /// <summary>
        ///     Copy the data inside into a new one.
        /// </summary>
        /// <returns></returns>
        public ISpectrum Clone()
        {
            return new ComplexSpectrum(Functions.Clone(RealArray), Functions.Clone(ImagArray), PulseCount);
        }

        /// <summary>
        ///     Try to add up another spectrum. The added spectrum must be of the same type.
        /// </summary>
        /// <param name="another"></param>
        public bool TryAbsorb(ISpectrum another)
        {
            if (Length() != another.Length() || !another.HasImag())
            {
                return false;
            }
            for (var i = 0; i < RealArray.Length; i++)
            {
                RealArray[i] += another.Real(i);
            }
            for (var i = 0; i < RealArray.Length; i++)
            {
                ImagArray[i] += another.Imag(i);
            }
            PulseCount += another.PulseCount;
            return true;
        }

        /// <summary>
        ///     Get the intensity of the data at a specified index
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Intensity(int index)
        {
            return RealArray[index]*RealArray[index] + ImagArray[index]*ImagArray[index];
        }

        /// <summary>
        ///     Get the real part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Real(int index)
        {
            return RealArray[index];
        }

        /// <summary>
        ///     Get the imag part of the accumulated data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double Imag(int index)
        {
            return ImagArray[index];
        }

        /// <summary>
        ///     Check whether the spectrum has the imag part or not.
        /// </summary>
        /// <returns></returns>
        public bool HasImag()
        {
            return true;
        }

        /// <summary>
        ///     Get the average intensity of the data at a specified index.
        /// </summary>
        /// <param name="index">The index of the data</param>
        /// <returns>The intensity at the input index</returns>
        public double AverageIntensity(int index)
        {
            return Intensity(index)/(PulseCount*PulseCount);
        }


        /// <summary>
        ///     Get the size of the data container
        /// </summary>
        /// <returns>The size of the data container</returns>
        public int Length()
        {
            return RealArray.Length;
        }


        /// <summary>
        ///     Get the string representation of the data at a specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string ToString(int index)
        {
            var imag = ImagArray[index];
            if (imag >= 0)
            {
                return RealArray[index] + "+" + imag + "j";
            }
            return RealArray[index] + "" + imag + "j";
        }

        /// <summary>
        ///     Get the string representation of the whole data array.
        /// </summary>
        /// <returns></returns>
        public string[] ToStringArray()
        {
            var array = new string[Length()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ToString(i);
            }
            return array;
        }
    }
}