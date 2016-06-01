using PhaseSonar.Maths;

namespace PhaseSonar.Correctors
{
    /// <summary>
    /// A modified accumulating mertz corrector which detects the reverse of amplitudes of the source pulse, 
    /// and flip them into the same direction
    /// </summary>
    public class AccFlipMertzCorrector : AccMertzCorrector
    {
        private readonly double[] _aux;

        /// <summary>
        /// Create an instance. <see cref="ICorrector{T}"/>
        /// </summary>
        /// <param name="apodizer"></param>
        /// <param name="fuzzyPulseLength"></param>
        /// <param name="zeroFillFactor"></param>
        /// <param name="centreSpan"></param>
        public AccFlipMertzCorrector(IApodizer apodizer, int fuzzyPulseLength, int zeroFillFactor, int centreSpan = 256)
            : base(apodizer, fuzzyPulseLength, zeroFillFactor, centreSpan)
        {
            _aux = new double[ZeroFilledLength];
        }

        /// <summary>
        ///     Write the index and the corresponding spectrum value
        /// </summary>
        /// <param name="i"></param>
        /// <param name="specValue"></param>
        protected override void WriteSpecPoint(int i, double specValue)
        {
            _aux[i] = specValue;
        }

        /// <summary>
        ///     Called when the correction is about to finish.
        /// </summary>
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