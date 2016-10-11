using System.Collections.Generic;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;

namespace PhaseSonar.Utils {
    /// <summary>
    ///     Extension Toolbox for IList{T}
    /// </summary>
    public static class ListExtension {
        /// <summary>
        ///     Return if the list is empty.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsEmpty<T>([NotNull] this IList<T> list) {
            return list.Count == 0;
        }

        /// <summary>
        ///     Return if the list is not empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool NotEmpty<T>([NotNull] this IList<T> list) {
            return list.Count != 0;
        }

        [NotNull]
        public static string Print<T>([NotNull] this IList<T> list) {
            var sb = new StringBuilder();
            sb.Append('[');
            foreach (var x1 in list) {
                sb.Append(x1);
                sb.Append(", ");
            }
            sb.Append(']');
            return sb.ToString();
        }

        public static void ToComplex([NotNull] this double[] doubles, [NotNull] Complex[] container) {
            for (var i = 0; i < doubles.Length; i++) {
                container[i] = doubles[i];
            }
        }

        [NotNull]
        public static Complex[] ToComplex([NotNull] this double[] doubles) {
            var container = new Complex[doubles.Length];
            for (var i = 0; i < doubles.Length; i++) {
                container[i] = doubles[i];
            }
            return container;
        }

        public static void Increase([NotNull] this Complex[] complexs, [NotNull] Complex[] another) {
            for (var i = 0; i < complexs.Length; i++) {
                complexs[i] += another[i];
            }
        }

        public static void FFT([NotNull] this Complex[] complexs) {
            Fourier.Forward(complexs, FourierOptions.Matlab);
        }

        public static void Phase([NotNull] this Complex[] complexs, double[] result) {
            for (var i = 0; i < complexs.Length; i++) {
                result[i] = complexs[i].Phase;
                // TODO MAYBE WRONG
            }
        }
    }
}