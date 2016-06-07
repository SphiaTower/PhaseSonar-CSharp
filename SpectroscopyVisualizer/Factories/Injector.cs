using System;
using System.Collections.Generic;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Writers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer.Factories
{
    public class Injector
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
        public static Accumulator<T> NewAccumulator<T>() where T : ISpectrum
        {
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var correctors = new List<ICorrector<T>>(threadNum);
            for (var i = 0; i < threadNum; i++)
            {
                correctors.Add(NewCorrector<T>());
            }
            return new ParallelAccumulator<T>(
                NewSlicer(),
                correctors);
//                return new SerialAccumulator(NewSlicer(),NewCorrector());
        }

        [NotNull]
        public static DisplayAdapter NewAdapter(CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView)
        {
            return new DisplayAdapter(view,horizontalAxisView, verticalAxisView, GeneralConfigurations.Get().DispPoints, SamplingConfigurations.Get().SamplingRate);
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
        public static ICorrector<T> NewCorrector<T>() where T : ISpectrum
        {
            var configs = GeneralConfigurations.Get();
            var fuzzyPeriodLength = (int) (SamplingConfigurations.Get().SamplingRate/configs.RepetitionRate);

            if (typeof(T) == typeof(RealSpectrum))
            {
                switch (CorrectorConfigurations.Get().CorrectorType)
                {
                    case CorrectorType.LinearMertz:
                        throw new NotImplementedException();
                    case CorrectorType.Mertz:
                        return
                            (ICorrector<T>)
                                new AccFlipMertzCorrector(NewApodizer(), fuzzyPeriodLength,
                                    CorrectorConfigurations.Get().ZeroFillFactor, CorrectorConfigurations.Get().CenterSpanLength/2);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            switch (CorrectorConfigurations.Get().CorrectorType)
            {
                case CorrectorType.Fake:
                    return
                        (ICorrector<T>)
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
            return new SampleProducer(NewSampler(), NewSampleCamera(cameraOn));
        }

        [NotNull]
        public static LocalProducer NewProducer(IEnumerable<string> paths)
        {
            return new LocalProducer(paths);
        }

        [NotNull]
        private static SpectroscopyVisualizer<T> NewConsumer<T>(IProducer producer, CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView, bool cameraOn)
            where T : ISpectrum
        {
            return new SpectroscopyVisualizer<T>(producer.BlockingQueue,
                view,
                NewAccumulator<T>(),
                NewAdapter(view,horizontalAxisView,verticalAxisView),
                NewCamera<T>(cameraOn));
        }

        [NotNull]
        public static SpectrumWriter<T> NewCamera<T>(bool on) where T : ISpectrum
        {
            return new SpectrumWriter<T>(GeneralConfigurations.Get().Directory, "captured-sq-aver-", on);
        }

        [NotNull]
        public static SampleWriter NewSampleCamera(bool on)
        {
            return new SampleWriter(GeneralConfigurations.Get().Directory, "binary-", on);
        }

        [NotNull]
        public static UiConsumer<double[]> NewConsumer(IProducer producer, CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView, bool cameraOn)
        {
            switch (CorrectorConfigurations.Get().CorrectorType)
            {
                case CorrectorType.Fake:
                    return NewConsumer<ComplexSpectrum>(producer, view, horizontalAxisView,verticalAxisView, cameraOn);
                case CorrectorType.LinearMertz:
                case CorrectorType.Mertz:
                    return NewConsumer<RealSpectrum>(producer, view, horizontalAxisView,verticalAxisView, cameraOn);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}