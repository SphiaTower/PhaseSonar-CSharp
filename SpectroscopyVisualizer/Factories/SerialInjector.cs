using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Factories
{
    public class SerialInjector
    {
        [NotNull]
        public static ISlicer NewSlicer()
        {
            var config = SliceConfigurations.Get();
            var crestFinder = NewCrestFinder();
            return config.CentreSlice
                ? new SymmetrySlicer(crestFinder)
                : new SimpleSlicer(crestFinder);
        }

        [NotNull]
        public static ICrestFinder NewCrestFinder()
        {
            var config = GeneralConfigurations.Get();
            return new IntelligentAbsoluteCrestFinder(
                config.RepetitionRate,
                SamplingConfigurations.Get().SamplingRate,
                SliceConfigurations.Get().PointsBeforeCrest,
                SliceConfigurations.Get().CrestAmplitudeThreshold
                );
        }

        [NotNull]
        public static Accumulator NewAccumulator()
        {
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var correctors = new List<ICorrector>(threadNum);
            for (var i = 0; i < threadNum; i++)
            {
                correctors.Add(NewCorrector());
            }
            return new ParallelAccumulator(
                NewSlicer(),
                correctors);
//                return new SerialAccumulator(NewSlicer(),NewCorrector());
        }

        [NotNull]
        public static DisplayAdapter NewAdapter(CanvasView view, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView)
        {
            return new DisplayAdapter(view, horizontalAxisView, verticalAxisView, GeneralConfigurations.Get().DispPoints,
                SamplingConfigurations.Get().SamplingRate);
        }

        [NotNull]
        public static Sampler NewSampler()
        {
            var configs = SamplingConfigurations.Get();
            return new Sampler(
                configs.DeviceName,
                configs.Channel.ToString(),
                configs.Range,
                configs.SamplingRate,
                configs.RecordLength
                );
        }

        [NotNull]
        public static ICorrector NewCorrector()
        {
            var configs = GeneralConfigurations.Get();
            var fuzzyPeriodLength = (int) (SamplingConfigurations.Get().SamplingRate/configs.RepetitionRate);

            switch (CorrectorConfigurations.Get().CorrectorType)
            {
                case CorrectorType.LinearMertz:
                    throw new NotImplementedException();
                case CorrectorType.Mertz:
                    return
                        new AccFlipMertzCorrector(NewApodizer(), fuzzyPeriodLength,
                            CorrectorConfigurations.Get().ZeroFillFactor,
                            CorrectorConfigurations.Get().CenterSpanLength/2);
                case CorrectorType.Fake:
                    return
                        new FakeCorrector(NewApodizer(), fuzzyPeriodLength, CorrectorConfigurations.Get().ZeroFillFactor);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IApodizer NewApodizer()
        {
            switch (CorrectorConfigurations.Get().ApodizerType)
            {
                case ApodizerType.Fake:
                    return new FakeApodizer();
                case ApodizerType.Triangular:
                    return new TriangulerApodizer();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [NotNull]
        public static SampleProducer NewProducer(bool cameraOn)
        {
            return new SampleProducer(NewSampler(), NewSampleWriter(cameraOn));
        }

        [NotNull]
        public static LocalProducer NewProducer(IEnumerable<string> paths)
        {
            return new LocalProducer(paths);
        }


        [NotNull]
        public static SpectrumWriter NewSpectrumWriter(bool on)
        {
            return new SpectrumWriter(GeneralConfigurations.Get().Directory, "captured-sq-aver-", on);
        }

        [NotNull]
        public static SampleWriter NewSampleWriter(bool on)
        {
            return new SampleWriter(GeneralConfigurations.Get().Directory, "binary-", on);
        }

        [NotNull]
        public static AbstractConsumer<double[]> NewConsumer(IProducer producer, CanvasView view,
            HorizontalAxisView horizontalAxisView, VerticalAxisView verticalAxisView, bool cameraOn)
        {
            return new SerialSpectroscopyVisualizer(producer.BlockingQueue,
                NewAccumulator(),
                NewAdapter(view, horizontalAxisView, verticalAxisView),
                NewSpectrumWriter(cameraOn));
        }
    }
}