using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CorrectorV2s.PulsePreprocessors;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers.WithoutReference.LockOn {
    /// <summary>
    /// lock on the center of mass
    /// </summary>
    public class LockComAccumulator : IAccumulator, ILockOnSeeker {
        private readonly BaseLockOnAccumulator _accumulatorImplementation;

        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="finder">A finder</param>
        /// <param name="slicer">A slicer</param>
        /// <param name="preprocessor"></param>
        /// <param name="corrector">A corrector</param>
        public LockComAccumulator([NotNull] ICrestFinder finder, ISlicer slicer, IPulsePreprocessor preprocessor, ICorrectorV2 corrector, double lockFreq, double lockScanFreqRadius, double sampleRateInMHz)  {
            _accumulatorImplementation = new BaseLockOnAccumulator(finder,slicer,preprocessor,corrector,lockFreq,lockScanFreqRadius,sampleRateInMHz,this);
        }

        /// <summary>
        ///     Process the pulse sequence and accumulate results of all pulses
        /// </summary>
        /// <param name="pulseSequence">The pulse sequence, without reference signals</param>
        /// <returns>The accumulated spectrum</returns>
        [NotNull]
        public AccumulationResult Process(double[] pulseSequence) {
            return _accumulatorImplementation.Process(pulseSequence);
        }

        public int SeekLockOnPoint(Complex[] spec, int start, int stop) {
            return Functions.FindCenterOfMass(spec, start, stop);
        }
    }
}