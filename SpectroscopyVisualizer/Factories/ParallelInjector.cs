using System;
using System.Collections.Generic;
using System.Windows.Controls;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using PhaseSonar.Analyzers;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Factories {
    /// <summary>
    ///     TODO: apply the abstract factory pattern
    /// </summary>
    public class ParallelInjector : IFactory {
        [NotNull]
        public virtual ISlicer NewSlicer() {
            return new SimpleSlicer(
                SliceConfigurations.Get().PointsBeforeCrest,
                NewRuler(),
                NewAligner());
        }

        [NotNull]
        public IRuler NewRuler() {
            switch (SliceConfigurations.Get().RulerType) {
                case RulerType.MinLength:
                    return new MinCommonLengthRuler();
                case RulerType.AverageLength:
                    return new AverageLengthRuler();
                case RulerType.FixLength:
                    return new FixLengtherRuler(SliceConfigurations.Get().FixedLength);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public IAligner NewAligner() {
            if (SliceConfigurations.Get().CrestAtCenter) {
                return new CenterAligner();
            }
            return new LeftAligner();
        }

        [NotNull]
        public virtual ICrestFinder NewCrestFinder() {
            var config = GeneralConfigurations.Get();
            var sliceConfig = SliceConfigurations.Get();
            ICrestFinder finder;
            if (sliceConfig.FindAbsoluteValue) {
                finder = new AbsoluteCrestFinder(
                    config.RepetitionRate, SamplingConfigurations.Get().SamplingRate,
                    sliceConfig.PointsBeforeCrest, sliceConfig.CrestAmplitudeThreshold);
            } else {
                finder = new SimpleCrestFinder(
                    config.RepetitionRate, SamplingConfigurations.Get().SamplingRate,
                    sliceConfig.PointsBeforeCrest, sliceConfig.CrestAmplitudeThreshold);
            }
            return sliceConfig.AutoAdjust ? new AutoAdjustCrestFinder(finder) : finder;
        }

        [NotNull]
        public IPulsePreprocessor NewPulsePreprocessor() {
            return new BalancePulsePreprocessor(CorrectorConfigurations.Get().ZeroFillFactor);
        }

        [NotNull]
        public IPulseSequenceProcessor NewPulseSequenceProcessor() {
            if (GeneralConfigurations.Get().ViewPhase) {
                return new PhaseReader(NewCrestFinder(), NewSlicer(), NewPulsePreprocessor(), NewPhaseExtractor());
            }
            return new Accumulator(NewCrestFinder(), NewSlicer(), NewPulsePreprocessor(), NewCorrector());
        }

        public DisplayAdapter NewAdapter(CanvasView view, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView,
            TextBox tbX, TextBox tbDelta) {
            return new DisplayAdapter(view, horizontalAxisView, verticalAxisView, tbX, tbDelta,
                GeneralConfigurations.Get().DispPoints,
                SamplingConfigurations.Get().SamplingRate);
        }


        [NotNull]
        public Sampler NewSampler() {
            var configs = SamplingConfigurations.Get();
            return new Sampler(configs.DeviceName, configs.Channel.ToString(), configs.Range, configs.SamplingRate,
                configs.RecordLength);
        }

        [NotNull]
        public IPhaseExtractor NewPhaseExtractor() {
            var config = CorrectorConfigurations.Get();
            switch (config.PhaseType) {
                case PhaseType.FullRange:
                    return new FourierOnlyPhaseExtractor();
                case PhaseType.CenterInterpolation:
                    return new CorrectCenterPhaseExtractor(NewApodizer(),
                        config.CenterSpanLength/2);
                case PhaseType.SpecifiedRange:
                    return new SpecifiedRangePhaseExtractor((int) config.RangeStart, (int) config.RangeEnd);
                case PhaseType.OldCenterInterpolation:
                    return new ClassicWrongPhaseExtractor(NewApodizer(),
                        config.CenterSpanLength/2);
                case PhaseType.SpecifiedFreqRange:
                    return new SpecifiedFreqRangePhaseExtractor(config.RangeStart, config.RangeEnd,
                        SamplingConfigurations.Get().SamplingRateInMHz);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public IPhaseSynthesizer NewPhaseSynthesizer() {
            if (CorrectorConfigurations.Get().RealSpec) {
                return new RealPhaseSynthesizer();
            } else {
                return new ComplexPhaseSynthesizer();
            }
        }

        [NotNull]
        private ICorrectorV2 NewCorrectorNoFlip() {
            switch (CorrectorConfigurations.Get().CorrectorType) {
                case CorrectorType.Mertz:
                    return new MertzCorrectorV2(NewPhaseExtractor(), NewApodizer(),NewPhaseSynthesizer());
                case CorrectorType.Fake:
                    return new FakeCorrectorV2(NewApodizer());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        [NotNull]
        public virtual ICorrectorV2 NewCorrector() {
            if (CorrectorConfigurations.Get().AutoFlip) {
                return new AutoFlipCorrectorV2(NewCorrectorNoFlip());
            } else {
                return NewCorrectorNoFlip();
            }
        }

        [NotNull]
        public IProducerV2<SampleRecord> NewProducer(int? targetCnt) {
            return new SampleProducerV2(NewSampler(),48, targetCnt);
        }

        [NotNull]
        public IProducerV2<SampleRecord> NewProducer([NotNull] IReadOnlyCollection<string> paths, bool compressed) {
            return new DiskProducerV2(paths,compressed);
        }

        [NotNull]
        public IWriterV2<TracedSpectrum> NewSpectrumWriter() {
            return new SpectrumWriterV2(GeneralConfigurations.Get().Directory, "[Sum][Magnitude]");
        }

        [NotNull]
        public IWriterV2<SampleRecord> NewSampleWriter() {
            return new SampleWriterV2(GeneralConfigurations.Get().Directory, "[Binary]");
        }

      

        [NotNull]
        public IConsumerV2 NewConsumer([NotNull] IProducerV2<SampleRecord> producer, [NotNull] DisplayAdapter adapter, IWriterV2<TracedSpectrum> writer, int? targetCnt) {
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var accumulators = new List<IPulseSequenceProcessor>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                accumulators.Add(NewPulseSequenceProcessor());
            }
            return new ParralelSpectroscopyVisualizerV2(producer.BlockingQueue, accumulators, adapter, writer,targetCnt);
        }

        [NotNull]
        private IApodizer NewApodizer() {
            switch (CorrectorConfigurations.Get().ApodizerType) {
                case ApodizerType.Fake:
                    return new FakeApodizer();
                case ApodizerType.Triangular:
                    return new TriangulerApodizer();
                case ApodizerType.Hann:
                    return new HannApodizer();
                case ApodizerType.Hamming:
                    return new HammingApodizer();
                case ApodizerType.Cosine:
                    return new CosineApodizer();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}