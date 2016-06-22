using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Maths
{
    /// <summary>
    ///     A set of static helper functions.
    /// </summary>
    public class Functions
    {
        /// <summary>
        ///     Copy array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="copy"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public static void CopyInto<T>(T[] array, int start, int count, T[] copy)
        {
            if (copy.Length < count)
            {
                throw new ArgumentException("array size not enough");
            }
//            for (int i = 0, j = start; i < count; i++, j++)
//            {
//                copy[i] = array[j];
//            }
            Array.Copy(array, start, copy, 0, count);
        }

        /// <summary>
        ///     Shallow clone array.
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] Clone<T>(T[] array)
        {
            var copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            return copy;
        }

        /// <summary>
        ///     Multiply 2 vectors in place.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="weight"></param>
        /// <exception cref="ArithmeticException"></exception>
        public static void MultiplyInPlace(double[] array, double[] weight)
        {
            var length = array.Length;
            if (weight.Length != length)
            {
                throw new ArithmeticException();
            }
            for (var i = 0; i < length; i++)
            {
                array[i] *= weight[i];
            }
        }

        /// <summary>
        ///     Add 2 arrays int a new one.
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        /// <exception cref="ArithmeticException"></exception>
        public static double[] Add(double[] array1, double[] array2)
        {
            var length = array1.Length;
            if (array2.Length != length)
            {
                throw new ArithmeticException("add failed, size doesn't match");
            }
            var result = new double[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = array1[i] + array2[i];
            }
            return result;
        }

        /// <summary>
        ///     Add 2 arrays in place.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="adder"></param>
        /// <exception cref="ArithmeticException"></exception>
        public static void AddTo([NotNull] double[] target, [NotNull] double[] adder)
        {
            var length = target.Length;
            if (adder.Length != length)
            {
                throw new ArithmeticException("add failed, size doesn't match");
            }
            for (var i = 0; i < length; i++)
            {
                target[i] += adder[i];
            }
        }

        /// <summary>
        ///     Add 2 arrays in place, regardless of the extra length.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="adder"></param>
        public static void ForceAddTo([NotNull] double[] target, [NotNull] double[] adder)
        {
            var length = target.Length;
            for (var i = 0; i < length; i++)
            {
                target[i] += adder[i];
            }
        }


        /// <summary>
        ///     Allocate linespace.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static double[] LineSpace(double start, double stop, int size)
        {
            var result = new double[size];
            LineSpaceInPlace(start, stop, size, result);
            return result;
        }

        /// <summary>
        ///     Allocate linespace in space.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="length"></param>
        /// <param name="container"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void LineSpaceInPlace(double start, double stop, int length, double[] container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container must not be null");
            }
            if (container.Length != length)
            {
                throw new ArgumentException("container size not matched");
            }
            var step = (stop - start)/(length - 1);
            for (var i = 0; i < length; i++)
            {
                container[i] = start + i*step;
            }
        }

        /// <summary>
        ///     Add arrays up.
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static double[] AddUp(List<double[]> arrays)
        {
            var length = arrays[0].Length;
            var result = new double[length];
            for (var i = 0; i < length; i++)
            {
                double t = 0;
                foreach (var array in arrays)
                {
                    t = t + array[i];
                }
                result[i] = t;
            }
            return result;
        }

        /// <summary>
        ///     Clear an array.
        /// </summary>
        /// <param name="array"></param>
        public static void Clear([NotNull] double[] array)
        {
            Array.Clear(array, 0, array.Length);
        }
    }
}