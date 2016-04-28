using FTIR.Maths;

namespace FTIR.Correctors
{
    public class AccFlipMertzCorrector : AccMertzCorrector
    {
        private readonly double[] _aux;

        public AccFlipMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan = 256)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
            _aux = new double[ZeroFilledLength];
        }

        protected override void WriteBuffer(int i, double specPoint)
        {
            _aux[i] = specPoint;
        }

        public override void Correct(double[] pulseSequence, int startIndex, int pulseLength, int pointsBeforeCrest)
        {
            base.Correct(pulseSequence, startIndex, pulseLength, pointsBeforeCrest);
            var sum = .0;
            for (var i = 0; i < _aux.Length/2; i++)
            {
                sum += _aux[i];
            }
            if (sum >= 0)
            {
                for (var i = 0; i < Output.Length; i++)
                {
                    Output[i] += _aux[i];
                }
            }
            else
            {
                for (var i = 0; i < Output.Length; i++)
                {
                    Output[i] += -_aux[i];
                }
            }
        }
    }
}