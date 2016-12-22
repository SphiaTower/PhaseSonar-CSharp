using System;
using System.Collections.Generic;
using System.Windows.Controls;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using PhaseSonar.Analyzers;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Maths;
using PhaseSonar.PhaseExtractors;
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
            return new Accumulator(NewCrestFinder(), NewSlicer(), NewPulsePreprocessor(), NewCorrector());
        }

        public SpectrumDisplayAdapter NewSpectrumAdapter(CanvasView view, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbX, TextBox tbDelta) {
            return new SpectrumDisplayAdapter(view, horizontalAxisView, verticalAxisView, tbX, tbDelta,
                GeneralConfigurations.Get().DispPoints,
                SamplingConfigurations.Get().SamplingRate, 0,
                (int) Math.Round(SamplingConfigurations.Get().SamplingRateInMHz/2));
        }

        public PhaseDisplayAdapter NewPhaseAdapter(CanvasView view, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbX, TextBox tbDelta) {
            return new PhaseDisplayAdapter(view, horizontalAxisView, verticalAxisView, tbX, tbDelta,
                GeneralConfigurations.Get().DispPoints,
                SamplingConfigurations.Get().SamplingRate, 0,
                (int) Math.Round(SamplingConfigurations.Get().SamplingRateInMHz/2));
        }


        public bool TryNewSampler(out Sampler newSampler) {
            var configs = SamplingConfigurations.Get();
            return Sampler.TryCreateSampler(out newSampler, configs.DeviceName, configs.Channel.ToString(),
                configs.Range, configs.SamplingRate,
                configs.RecordLength);
        }

        [NotNull]
        public IPhaseExtractor NewPhaseExtractor() {
            if (GeneralConfigurations.Get().ViewPhase) {
                return new FourierOnlyPhaseExtractor();
            }
            var config = CorrectorConfigurations.Get();
            var misConfig = MiscellaneousConfigurations.Get();
            switch (config.PhaseType) {
                case PhaseType.FullRange:
                    return new FourierOnlyPhaseExtractor();
                case PhaseType.CenterInterpolation:
                    return new CorrectCenterPhaseExtractor(NewApodizer(),
                        config.CenterSpanLength/2);
                case PhaseType.SpecifiedRange:
                    return new SpecifiedRangePhaseExtractor((int) config.RangeStart, (int) config.RangeEnd,
                        misConfig.MinFlatPhasePtsNumCnt, misConfig.MaxPhaseStd);
                case PhaseType.OldCenterInterpolation:
                    return new ClassicWrongPhaseExtractor(NewApodizer(),
                        config.CenterSpanLength/2);
                case PhaseType.SpecifiedFreqRange:
                    return new SpecifiedFreqRangePhaseExtractor(config.RangeStart, config.RangeEnd,
                        SamplingConfigurations.Get().SamplingRateInMHz, misConfig.MinFlatPhasePtsNumCnt,
                        misConfig.MaxPhaseStd);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public virtual ICorrectorV2 NewCorrector() {
            if (CorrectorConfigurations.Get().AutoFlip) {
                return new AutoFlipCorrectorV2(NewCorrectorNoFlip());
            }
            return NewCorrectorNoFlip();
        }

        public bool TryNewSampleProducer(out IProducerV2<SampleRecord> newProducer, int? targetCnt = null) {
            Sampler sampler;
            if (TryNewSampler(out sampler)) {
                newProducer = new SampleProducerV2(sampler, GeneralConfigurations.Get().QueueSize, targetCnt);
                if (GeneralConfigurations.Get().SaveSample) {
                    var newSampleWriter = FactoryHolder.Get().NewSampleWriter();
                    newProducer.NewProduct += record => { newSampleWriter.Write(record); };
                }
                return true;
            }
            newProducer = null;
            return false;
        }

        [NotNull]
        public IProducerV2<SampleRecord> NewProducer([NotNull] IReadOnlyCollection<string> paths, bool compressed) {
            return new DiskProducerV2(paths, compressed);
        }

        [NotNull]
        public IWriterV2<TracedSpectrum> NewSpectrumWriter() {
            return new SpectrumWriterV2(GeneralConfigurations.Get().Directory, "[Sum]",
                GeneralConfigurations.Get().SaveType);
        }

        [NotNull]
        public IWriterV2<SampleRecord> NewSampleWriter() {
            return new SampleWriterV2(GeneralConfigurations.Get().Directory, "[Binary]");
        }


        [NotNull]
        public IConsumerV2 NewConsumer([NotNull] IProducerV2<SampleRecord> producer, [NotNull] DisplayAdapterV2 adapter,
            int? targetCnt) {
            var timeout = MiscellaneousConfigurations.Get().WaitEmptyProducerMsTimeout;

            var configurations = GeneralConfigurations.Get();
            if (configurations.ViewPhase) {
                return new PhaseVisualizer(producer.BlockingQueue, NewPhaseReader(), adapter as PhaseDisplayAdapter,
                    null, timeout,
                    targetCnt);
            }
            var threadNum = configurations.ThreadNum;
            if (SliceConfigurations.Get().Reference) {
                var splitters = new List<IRefPulseSequenceProcessor>(threadNum);
                for (var i = 0; i < threadNum; i++) {
                    splitters.Add(NewRefProcessor());
                }
                return new RefSpectroscopyVisualizer(producer.BlockingQueue, splitters,
                    adapter as SpectrumDisplayAdapter, NewSpectrumWriter(), timeout, targetCnt, configurations.SaveSpec,
                    configurations.SaveAcc);
            }
            var accumulators = new List<IPulseSequenceProcessor>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                accumulators.Add(NewPulseSequenceProcessor());
            }
            return new ParralelSpectroscopyVisualizerV2(producer.BlockingQueue, accumulators,
                adapter as SpectrumDisplayAdapter, NewSpectrumWriter(), timeout, targetCnt, configurations.SaveSpec,
                configurations.SaveAcc);
        }

        [NotNull]
        private IPhaseReader NewPhaseReader() {
            return new PhaseReader(NewCrestFinder(), NewSlicer(), NewPulsePreprocessor(), NewPhaseExtractor());
        }

        [NotNull]
        private Splitter NewRefProcessor() {
            return new Splitter(NewCrestFinder(),
                new RefSlicer(SliceConfigurations.Get().PointsBeforeCrest, NewRuler(), NewAligner()),
                NewPulsePreprocessor(), NewCorrector());
        }

        [NotNull]
        public IPhaseSynthesizer NewPhaseSynthesizer() {
            if (CorrectorConfigurations.Get().RealSpec) {
                return new RealPhaseSynthesizer();
            }
            return new ComplexPhaseSynthesizer();
        }

        [NotNull]
        private ICorrectorV2 NewCorrectorNoFlip() {
            switch (CorrectorConfigurations.Get().CorrectorType) {
                case CorrectorType.Mertz:
                    return new MertzCorrectorV2(NewPhaseExtractor(), NewApodizer(), NewPhaseSynthesizer());
                case CorrectorType.Fake:
                    return new FakeCorrectorV2(NewApodizer());
                default:
                    throw new ArgumentOutOfRangeException();
            }
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