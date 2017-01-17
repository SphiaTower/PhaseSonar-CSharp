using System.Collections.Generic;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.Scripting.Hosting;

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

        public static void AlignIncrease([NotNull] this Complex[] complexs, int dipIndex1, [NotNull] Complex[] another,int dipIndex2) {
            int diff = dipIndex1 - dipIndex2;
            var length = complexs.Length;
            if (diff>0) {
                for (int i = diff,j=0; i < length; i++,j++) {
                    complexs[i] += another[j];
                }
                for (int i = 0,j=length-diff; i < diff; i++,j++) {
                    complexs[i] += another[j];
                }
            }else if (diff == 0) {
                complexs.Increase(another);
            } else {
                for (int i = 0, j = -diff; j < length; i++, j++) {
                    complexs[i] += another[j];
                }
                for (int i = length+diff, j = 0; i<length; i++, j++) {
                    complexs[i] += another[j];
                }
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