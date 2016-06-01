﻿using System;
using System.Collections.Generic;
using PhaseSonar.Analyzers;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using PhaseSonar.Slicers;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Controllers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer.Factories
{
    public class ParallelInjector
    {
        [NotNull]
        public static ISlicer NewSlicer()
        {
            var config = SliceConfigs.Get();
            var crestFinder = NewCrestFinder();
            return config.CentreSlice
                ? new SymmetrySlicer(crestFinder)
                : new SimpleSlicer(crestFinder);
        }

        [NotNull]
        public static ICrestFinder NewCrestFinder()
        {
            var config = Configurations.Get();
            return new IntelligentAbsoluteCrestFinder(
                config.RepetitionRate,
                SamplingConfigs.Get().SamplingRate,
                SliceConfigs.Get().PointsBeforeCrest,
                SliceConfigs.Get().CrestAmplitudeThreshold
                );
        }

        [NotNull]
        public static SerialAccumulator<T> NewAccumulator<T>() where T : ISpectrum
        {
            var threadNum = Configurations.Get().ThreadNum;
            return new SerialAccumulator<T>(
                NewSlicer(),
                NewCorrector<T>());
//                return new SerialAccumulator(NewSlicer(),NewCorrector());
        }

        [NotNull]
        public static DisplayAdapter NewAdapter(CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView)
        {
            return new DisplayAdapter(view,horizontalAxisView, verticalAxisView, Configurations.Get().DispPoints, SamplingConfigs.Get().SamplingRate);
        }

        [NotNull]
        public static Sampler NewSampler()
        {
            var configs = SamplingConfigs.Get();
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
            var configs = Configurations.Get();
            var fuzzyPeriodLength = (int) (SamplingConfigs.Get().SamplingRate/configs.RepetitionRate);

            if (typeof(T) == typeof(RealSpectrum))
            {
                switch (CorrectorConfigs.Get().CorrectorType)
                {
                    case CorrectorType.LinearMertz:
                        throw new NotImplementedException();
                    case CorrectorType.Mertz:
                        return
                            (ICorrector<T>)
                                new AccFlipMertzCorrector(NewApodizer(), fuzzyPeriodLength,
                                    CorrectorConfigs.Get().ZeroFillFactor, CorrectorConfigs.Get().CenterSpanLength/2);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            switch (CorrectorConfigs.Get().CorrectorType)
            {
                case CorrectorType.Fake:
                    return
                        (ICorrector<T>)
                            new FakeCorrector(NewApodizer(), fuzzyPeriodLength, CorrectorConfigs.Get().ZeroFillFactor);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IApodizer NewApodizer()
        {
            switch (CorrectorConfigs.Get().ApodizerType)
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
        private static ParallelSpectroscopyVisualizer<T> NewConsumer<T>(IProducer producer, CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView, bool cameraOn)
            where T : ISpectrum
        {
            var threadNum = Configurations.Get().ThreadNum;
            List<SerialAccumulator<T>> list = new List<SerialAccumulator<T>>(threadNum);
            for (int i = 0; i < threadNum; i++)
            {
               list.Add(NewAccumulator<T>());
            }
            return new ParallelSpectroscopyVisualizer<T>(producer.BlockingQueue, view, list, NewAdapter(view,horizontalAxisView,verticalAxisView),
                NewCamera<T>(cameraOn));
        }

        [NotNull]
        public static SpectrumCamera<T> NewCamera<T>(bool on) where T : ISpectrum
        {
            return new SpectrumCamera<T>(Configurations.Get().Directory, "captured-sq-aver-", on);
        }

        [NotNull]
        public static SampleCamera NewSampleCamera(bool on)
        {
            return new SampleCamera(Configurations.Get().Directory, "binary-", on);
        }

        [NotNull]
        public static UiConsumer<double[]> NewConsumer(IProducer producer, CanvasView view,HorizontalAxisView horizontalAxisView,VerticalAxisView verticalAxisView, bool cameraOn)
        {
            switch (CorrectorConfigs.Get().CorrectorType)
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