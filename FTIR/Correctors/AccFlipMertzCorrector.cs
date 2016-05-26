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

        protected override void OnCorrected()
        {
            var sum = .0;
            for (var i = 0; i < _aux.Length / 2; i++) {
                sum += _aux[i];
            }
            if (sum >= 0) {
                for (var i = 0; i < OutputLength; i++) {
                    SpectrumBuffer.AmplitudeArray[i] += _aux[i];
                }
            } else {
                for (var i = 0; i < OutputLength; i++) {
                    SpectrumBuffer.AmplitudeArray[i] += -_aux[i];
                }
            }
            base.OnCorrected();
        }
    }
}