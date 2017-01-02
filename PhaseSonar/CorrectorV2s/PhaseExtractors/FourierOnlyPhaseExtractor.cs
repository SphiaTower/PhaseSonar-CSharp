using System.Numerics;
using JetBrains.Annotations;
using MathNet.Numerics.IntegralTransforms;
using PhaseSonar.Maths;

namespace PhaseSonar.PhaseExtractors {
    public class FourierOnlyPhaseExtractor : IPhaseExtractor {
        private Complex[] _complexContainer;
        private double[] _phaseArray;


        public double[] GetPhase(double[] symmetryPulse, [CanBeNull] Complex[] correspondSpectrum) {
            if (_phaseArray == null) {
                _phaseArray = new double[symmetryPulse.Length/2];
            }

            Complex[] complexSpectrum;
            if (correspondSpectrum == null) {
                if (_complexContainer == null) {
                    _complexContainer = new Complex[symmetryPulse.Length];
                }
                Functions.ToComplexRotate(symmetryPulse, _complexContainer);
                Fourier.Forward(_complexContainer, FourierOptions.Matlab);
                complexSpectrum = _complexContainer;
            } else {
                complexSpectrum = correspondSpectrum;
            }

            RawSpectrumReady?.Invoke(complexSpectrum);

            //            symmetryPulse.ToComplex(_complexContainer);
            //            _rotator.Rotate(_complexContainer);

            // rotate & to complex

            for (var i = 0; i < _phaseArray.Length; i++) {
                _phaseArray[i] = complexSpectrum[i].Phase;
            }
            RawPhaseReady?.Invoke(_phaseArray);
            return _phaseArray;
        }

        public event SpectrumReadyEventHandler RawSpectrumReady;
        public event PhaseReadyEventHandler RawPhaseReady;
    }
}