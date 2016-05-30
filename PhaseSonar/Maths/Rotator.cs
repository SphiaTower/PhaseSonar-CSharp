using System;

namespace PhaseSonar.Maths
{
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

        public void Rotate(double[] array)
        {
            var length = array.Length;
            var half = length/2;

            Funcs.CopyInto(array, 0, half, Allocate(half));

            for (int i = 0, j = half; i < half; i++, j++)
            {
                array[i] = array[j];
            }
            for (int i = half, j = 0; i < length; i++, j++)
            {
                array[i] = _aux[j];
            }
        }

        public void SymmetrizeInPlace(double[] pulse, int crestIndex)
        {
            //var crestIndex = Funcs.FindCrestIndex(pulse); // todo abs/signed
            var length = pulse.Length;
            var centerIndex = length/2;
            if (crestIndex == centerIndex)
            {
            }
            else if (crestIndex < centerIndex)
            {
                var auxLength = centerIndex - crestIndex;
                Funcs.CopyInto(pulse, centerIndex + crestIndex, auxLength, Allocate(auxLength));
                for (var r = length - 1; r > auxLength; r--)
                {
                    pulse[r] = pulse[r - auxLength];
                }
                for (var i = 0; i < auxLength; i++)
                {
                    pulse[i] = _aux[i];
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("try to symmetrize when crest is at tail");
            }
        }
    }
}