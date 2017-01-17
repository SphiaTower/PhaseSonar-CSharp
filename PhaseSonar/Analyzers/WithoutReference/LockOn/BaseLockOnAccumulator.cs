using System;
using System.Numerics;
using JetBrains.Annotations;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CorrectorV2s.PulsePreprocessors;
using PhaseSonar.CrestFinders;
using PhaseSonar.Slicers;
using PhaseSonar.Utils;

namespace PhaseSonar.Analyzers.WithoutReference.LockOn {
    public class BaseLockOnAccumulator : Accumulator {
        private readonly double _lockFreq;
        private readonly double _lockScanFreqRadius;
        private readonly double _sampleRateInMHz;
        private readonly ILockOnSeeker _seeker;
        private int? _lockIndex;
        private int _scanIndexRadius;
        /// <summary>
        ///     Create an accumulator
        /// </summary>
        /// <param name="finder">A finder</param>
        /// <param name="slicer">A slicer</param>
        /// <param name="preprocessor"></param>
        /// <param name="corrector">A corrector</param>
        public BaseLockOnAccumulator([NotNull] ICrestFinder finder, ISlicer slicer, IPulsePreprocessor preprocessor, ICorrectorV2 corrector,double lockFreq, double lockScanFreqRadius,double sampleRateInMHz,ILockOnSeeker seeker ) : base(finder, slicer, preprocessor, corrector) {
            _lockFreq = lockFreq;
            _lockScanFreqRadius = lockScanFreqRadius;
            _sampleRateInMHz = sampleRateInMHz;
            _seeker = seeker;
        }

        protected override Complex[] Accumulate(Complex[] accumulatedSpectrum, Complex[] correctedSpectrum) {
            if (!_lockIndex.HasValue) {
                _lockIndex = (int)Math.Round(_lockFreq*correctedSpectrum.Length/(_sampleRateInMHz/2));
                _scanIndexRadius = (int)Math.Round(_lockScanFreqRadius*correctedSpectrum.Length/(_sampleRateInMHz/2));
            }

            var dipIndex = _seeker.SeekLockOnPoint(correctedSpectrum,_lockIndex.Value-_scanIndexRadius,_lockIndex.Value+_scanIndexRadius);
            if (accumulatedSpectrum==null) {
                accumulatedSpectrum = new Complex[correctedSpectrum.Length];
            }
            accumulatedSpectrum.AlignIncrease(_lockIndex.Value, correctedSpectrum, dipIndex);
            return accumulatedSpectrum;
        }
    }
}