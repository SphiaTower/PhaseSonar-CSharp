using System;
using System.Collections.Generic;
using FTIR.Utils;
using JetBrains.Annotations;

namespace FTIR.Maths
{
    public class Funcs
    {

        public static void CopyInto<T>(T[] array, int start, int count, T[] copy)
        {
            if (copy.Length < count)
            {
                throw new ArgumentException("array size not enough");
            }
            for (int i = 0, j = start; i < count; i++, j++)
            {
                copy[i] = array[j];
            }
        }

        public static T[] Clone<T>(T[] array)
        {
            var copy = new T[array.Length];
            Array.Copy(array,copy,array.Length);
            return copy;
        }

        public static T[] CopyRange<T>(T[] array, int start, int count)
        {
            var copy = new T[count];
            for (int i = 0, j = start; i < count; i++, j++)
            {
                copy[i] = array[j];
            }
            return copy;
        }


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

        public static void AddTo([NotNull]double[] target, [NotNull]double[] adder)
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
        public static void ForceAddTo([NotNull]double[] target, [NotNull]double[] adder) {
            var length = target.Length;
            for (var i = 0; i < length; i++) {
                target[i] += adder[i];
            }
        }
        public static T[] Rotate<T>(T[] array)
        {
            var length = array.Length;
            var rotated = new T[length];
            var half = length/2;
            for (var i = 0; i < half; i++)
            {
                rotated[i] = array[half + i];
            }
            for (var i = half; i < length; i++)
            {
                rotated[i] = array[i - half];
            }
            return rotated;
        }


        public static double[] LineSpace(double start, double stop, int size)
        {
            var result = new double[size];
            LineSpaceInPlace(start, stop, size, result);
            return result;
        }

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

        public static void Clear([NotNull]double[] array)
        {
            Array.Clear(array,0,array.Length);
        }
    }
}