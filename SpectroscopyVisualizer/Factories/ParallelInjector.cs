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
                    return new FixLengtherRuler(100);
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
            return new AbsoluteCrestFinder( // todo change back
                config.RepetitionRate, SamplingConfigurations.Get().SamplingRate,
                SliceConfigurations.Get().PointsBeforeCrest, SliceConfigurations.Get().CrestAmplitudeThreshold);
        }

        [NotNull]
        public IPulsePreprocessor NewPulsePreprocessor() {
            return new BalancePulsePreprocessor(CorrectorConfigurations.Get().ZeroFillFactor);
        }

        [NotNull]
        public IPulseSequenceProcessor NewPulseSequenceProcessor() {
            return new Accumulator(NewCrestFinder(), NewSlicer(), NewPulsePreprocessor(), NewCorrector());
        }

        public DisplayAdapter NewAdapter(CanvasView view, HorizontalAxisView horizontalAxisView, VerticalAxisView verticalAxisView,
            TextBox tbX, TextBox tbDelta) {
            return new DisplayAdapter(view, horizontalAxisView, verticalAxisView, tbX, tbDelta, GeneralConfigurations.Get().DispPoints,
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
                    return new SpecifiedRangePhaseExtractor(config.RangeStart,config.RangeEnd);
                case PhaseType.OldCenterInterpolation:
                    return new ClassicWrongPhaseExtractor(NewApodizer(),
                        config.CenterSpanLength/2);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public ICorrectorV2 NewCorrectorNoFlip() {

            switch (CorrectorConfigurations.Get().CorrectorType) {
                case CorrectorType.LinearMertz:
                    throw new NotImplementedException();
                case CorrectorType.Mertz:
                    return new MertzCorrectorV2(NewPhaseExtractor(), NewApodizer());

                case CorrectorType.Fake:
                    return new FakeCorrectorV2(NewApodizer());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public virtual ICorrectorV2 NewCorrector() {
            return new AutoFlipCorrectorV2(NewCorrectorNoFlip());
        }

        [NotNull]
        public virtual SampleProducer NewProducer(bool cameraOn) {
            return new SampleProducer(NewSampler(), NewSampleWriter(cameraOn));
        }

        [NotNull]
        public virtual DiskProducer NewProducer(IEnumerable<string> paths, bool compressed) {
            return new DiskProducer(paths, compressed);
        }


        [NotNull]
        public SpectrumWriter NewSpectrumWriter(bool on) {
            return new SpectrumWriter(GeneralConfigurations.Get().Directory, "[Average][Spectrum]", on);
        }

        [NotNull]
        public SampleWriter NewSampleWriter(bool on) {
            return new SampleWriter(GeneralConfigurations.Get().Directory, "[Binary]", on);
        }

        [NotNull]
        public AbstractConsumer<SampleRecord> NewConsumer([NotNull] IProducer<SampleRecord> producer,
            [NotNull] DisplayAdapter adapter, SpectrumWriter writer) {
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var accumulators = new List<IPulseSequenceProcessor>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                accumulators.Add(NewPulseSequenceProcessor());
            }
            return new ParallelSpectroscopyVisualizer(producer.BlockingQueue, accumulators, adapter, writer);
        }

        [NotNull]
        private IApodizer NewApodizer() {
            switch (CorrectorConfigurations.Get().ApodizerType) {
                case ApodizerType.Fake:
                    return new FakeApodizer();
                case ApodizerType.Triangular:
                    return new TriangulerApodizer();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}