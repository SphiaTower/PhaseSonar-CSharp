using System;

namespace PhaseSonar.Maths
{
    /// <summary>
    ///     A Rotator takes an array as input and do fftshift on it or symmetrize it.
    /// </summary>
    public class Rotator
    {
        private double[] _aux;

        private double[] Allocate(int size)
        {
            if (_aux == null || _aux.Length < size)
            {
                _aux = new double[size];
            }
            return _aux;
        }

        /// <summary>
        ///     Swap the two halves of the array, like a fftshift operation
        /// </summary>
        /// <param name="array">The array to be rotated</param>
        public void Rotate(double[] array)
        {
            var length = array.Length;
            var half = length/2;

            Functions.CopyInto(array, 0, half, Allocate(half));

            for (int i = 0, j = half; i < half; i++, j++)
            {
                array[i] = array[j];
            }
            for (int i = half, j = 0; i < length; i++, j++)
            {
                array[i] = _aux[j];
            }
        }

        /// <summary>
        ///     Rotate the array to center the crest of array
        /// </summary>
        /// <param name="array">The array to be symmetrized</param>
        /// <param name="crestIndex">The index to be rotated to the center of the array</param>
        public bool TrySymmetrize(double[] array, int crestIndex)
        {
            var length = array.Length;
            var centerIndex = length/2;
            if (crestIndex == centerIndex)
            {
            }
            else if (crestIndex < centerIndex)
            {
                var auxLength = centerIndex - crestIndex;
                Functions.CopyInto(array, centerIndex + crestIndex, auxLength, Allocate(auxLength));
                for (var r = length - 1; r > auxLength; r--)
                {
                    array[r] = array[r - auxLength];
                }
                for (var i = 0; i < auxLength; i++)
                {
                    array[i] = _aux[i];
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}