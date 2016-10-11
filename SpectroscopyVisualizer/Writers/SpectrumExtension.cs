using System;
using JetBrains.Annotations;
using PhaseSonar.Correctors;

namespace SpectroscopyVisualizer.Writers {
    /// <summary>
    ///     An extension for <see cref="ISpectrum" />
    /// </summary>
    public static class SpectrumExtension {
        /// <summary>
        ///     Get the string representation of the data at a specified index.
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        [NotNull]
        public static string ToStringMagnitude([NotNull] this ISpectrum spectrum, int index) {
            return spectrum.Magnitude(index).ToString();
        }

        [NotNull]
        public static string ToStringComplex([NotNull] this ISpectrum spectrum, int index) {
            if (spectrum.HasImag()) {
                return spectrum.Real(index) + "\t" + spectrum.Imag(index);
            }
            return spectrum.Real(index).ToString();
        }
        [NotNull]
        public static string ToStringIntensity([NotNull] this ISpectrum spectrum, int index) {
           return spectrum.Intensity(index).ToString();
        }

        [NotNull]
        public static string ToStringPhase([NotNull] this ISpectrum spectrum, int index) {
           return spectrum.Phase(index).ToString();
        }
        [NotNull]
        public static string ToStringReal([NotNull] this ISpectrum spectrum, int index) {
           return spectrum.Real(index).ToString();
        }
        [NotNull]
        public static string ToStringImag([NotNull] this ISpectrum spectrum, int index) {
           return spectrum.Imag(index).ToString();
        }

        /// <summary>
        ///     Get the string representation of the whole data array.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public static string[] ToStringArray([NotNull] this ISpectrum spectrum,Func<ISpectrum,int,string> toStringFunc) {
            var array = new string[spectrum.Length()];
            for (var i = 0; i < array.Length; i++) {
                array[i] = toStringFunc(spectrum, i);
            }
            return array;
        }

        [NotNull]
        public static string Enclose(this string content) {
            return "[" + content + "]";
        }

        [NotNull]
        public static string Enclose(this int content) {
            return "[" + content + "]";
        }

        [NotNull]
        public static string Enclose(this string content, string key) {
            return "[" + key + "-" + content + "]";
        }

        [NotNull]
        public static string Enclose(this int content, string key) {
            return "[" + key + "-" + content + "]";
        }
    }
}