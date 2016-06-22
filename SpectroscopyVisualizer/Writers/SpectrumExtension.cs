using PhaseSonar.Correctors;

namespace SpectroscopyVisualizer.Writers
{
    /// <summary>
    /// An extension for <see cref="ISpectrum"/>
    /// </summary>
    public static class SpectrumExtension
    {
        /// <summary>
        ///     Get the string representation of the data at a specified index.
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ToString(this ISpectrum spectrum, int index)
        {
            if (spectrum.HasImag())
            {
                return spectrum.Real(index) + "\t" + spectrum.Imag(index);
            }
            return spectrum.Real(index).ToString();
        }

        /// <summary>
        ///     Get the string representation of the whole data array.
        /// </summary>
        /// <returns></returns>
        public static string[] ToStringArray(this ISpectrum spectrum)
        {
            var array = new string[spectrum.Length()];
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = ToString(spectrum, i);
            }
            return array;
        }
    }
}