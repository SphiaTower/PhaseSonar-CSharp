using System;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;

namespace PhaseSonar.Maths {
    /// <summary>
    ///     A set of static helper functions.
    /// </summary>
    public static class Functions {
        /// <summary>
        ///     Copy array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="copy"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public static void CopyInto<T>([NotNull] T[] array, int start, int count, [NotNull] T[] copy) {
            if (copy.Length < count) {
                throw new ArgumentException("array size not enough");
            }
//            for (int i = 0, j = start; i < count; i++, j++)
//            {
//                copy[i] = array[j];
//            }
            Array.Copy(array, start, copy, 0, count);
        }

        public static void ToComplex([NotNull] double[] doubles, Complex[] container) {
            for (var i = 0; i < doubles.Length; i++) {
                container[i] = new Complex(doubles[i], 0);
            }
        }

        public static void ToComplexRotate([NotNull] double[] symmetryPulse, IList<Complex> array) {
            var length = symmetryPulse.Length;
            var j = 0;
            for (var i = length / 2; i < length; i++, j++) {
                array[j] = symmetryPulse[i];
            }
            for (var i = 0; i < length / 2; i++, j++) {
                array[j] = symmetryPulse[i];
            }
        }

        /// <summary>
        ///     Calculate the length after zero filling.
        /// </summary>
        /// <param name="dataLength">The length of data</param>
        /// <param name="zeroFillFactor">The zero fill factor</param>
        /// <returns></returns>
        public static int CalZeroFilledLength(int dataLength, int zeroFillFactor) {
            return (int) Math.Pow(2, (int) Math.Log(dataLength, 2) + zeroFillFactor);
        }
        public static readonly double CIRCLE = Math.PI * 2;

        public static void UnwrapInPlace([NotNull] double[] phase) {
            var length = phase.Length;
            var prev = phase[0];
            for (int i = 1; i < length; i++) {
                while (prev - phase[i] >= Math.PI) {
                    phase[i] += CIRCLE;
                }
                while (phase[i]-prev>= Math.PI) {
                    phase[i] -= CIRCLE;
                }
                prev = phase[i];
            }
        }

        public static double[] Unwrap([NotNull] double[] phase) {
            double[] unwrapDoubles = phase.Clone() as double[];
            UnwrapInPlace(unwrapDoubles);
            return unwrapDoubles;
        }

        /// <summary>
        ///     Calculate the sum of a specified range
        /// </summary>
        /// <param name="array">The input array</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="length">The length of the range.</param>
        /// <returns></returns>
        public static double RangeSum(IReadOnlyList<double> array, int startIndex, int length) {
            var sum = .0;
            for (var i = startIndex; i < startIndex + length; i++) {
                sum += array[i];
            }
            return sum;
        }

        public static double Average(double[] array, int start, int count) {
            double sum = 0;
            for (var i = start; i < start + count; i++) {
                sum += array[i];
            }
            return sum/count;
        }

        public static double Phase(double real, double imag) {
            if (Math.Abs(real) > 0.0000001) {
                return Math.Atan(imag/real);
            }
            if (imag > 0) {
                return Math.PI/2;
            }
            if (imag < 0) {
                return -Math.PI/2;
            }
            return 0;
        }

        public static double Phase(Complex complex) {
            return Phase(complex.Real, complex.Imaginary);
//            return Math.Atan2(complex.Imaginary,complex.Real);
        }

        /// <summary>
        ///     Shallow clone array.
        /// </summary>
        /// <param name="array"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [NotNull]
        public static T[] Clone<T>([NotNull] T[] array) {
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
        public static void MultiplyInPlace([NotNull] double[] array, [NotNull] double[] weight) {
            var length = array.Length;
            if (weight.Length != length) {
                throw new ArithmeticException();
            }
            for (var i = 0; i < length; i++) {
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
        [NotNull]
        public static double[] Add([NotNull] double[] array1, [NotNull] double[] array2) {
            var length = array1.Length;
            if (array2.Length != length) {
                throw new ArithmeticException("add failed, size doesn't match");
            }
            var result = new double[length];
            for (var i = 0; i < length; i++) {
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
        public static void AddTo([NotNull] double[] target, [NotNull] double[] adder) {
            var length = target.Length;
            if (adder.Length != length) {
                throw new ArithmeticException("add failed, size doesn't match");
            }
            for (var i = 0; i < length; i++) {
                target[i] += adder[i];
            }
        }

        /// <summary>
        ///     Add 2 arrays in place, regardless of the extra length.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="adder"></param>
        public static void ForceAddTo([NotNull] double[] target, [NotNull] double[] adder) {
            var length = target.Length;
            for (var i = 0; i < length; i++) {
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
        public static double[] LineSpace(double start, double stop, int size) {
            var result = new double[size];
            LineSpaceInPlace(start, stop, size, result);
            return result;
        }

        /// <summary>
        ///     Allocate linespace in space. [start, stop]
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="length"></param>
        /// <param name="container"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void LineSpaceInPlace(double start, double stop, int length, double[] container) {
            if (container == null) {
                throw new ArgumentNullException("container must not be null");
            }
            if (container.Length != length) {
                throw new ArgumentException("container size not matched");
            }
            var step = (stop - start)/(length - 1);
            for (var i = 0; i < length; i++) {
                container[i] = start + i*step;
            }
        }

        /// <summary>
        ///     Add arrays up.
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        public static double[] AddUp(List<double[]> arrays) {
            var length = arrays[0].Length;
            var result = new double[length];
            for (var i = 0; i < length; i++) {
                double t = 0;
                foreach (var array in arrays) {
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
        public static void Clear([NotNull] double[] array) {
            Array.Clear(array, 0, array.Length);
        }

        public static void FindMinMax([NotNull] double[] nums, out double min, out double max) {
            /*min = double.MaxValue;
            max = double.MinValue;
            foreach (var y in nums) {
                if (y > max) {
                    max = y;
                } else if (y < min) {
                    min = y;
                }
            }*/
            var length = nums.Length;
            if (length == 0) {
                throw new ArgumentOutOfRangeException();
            }
            int i;
            if (length%2 == 1) {
                max = min = nums[0];
                i = 1;
            } else {
                if (nums[0] > nums[1]) {
                    max = nums[0];
                    min = nums[1];
                } else {
                    max = nums[1];
                    min = nums[0];
                }
                i = 2;
            }
            for (; i < length; i += 2) {
                var num1 = nums[i];
                var num2 = nums[i + 1];
                if (num1 > num2) {
                    max = Math.Max(max, num1);
                    min = Math.Min(min, num2);
                } else {
                    max = Math.Max(max, num2);
                    min = Math.Min(min, num1);
                }
            }
        }
    }
}