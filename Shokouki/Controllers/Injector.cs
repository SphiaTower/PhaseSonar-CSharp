using System.Collections.Generic;
using FTIR.Analyzers;
using FTIR.Correctors;
using FTIR.Maths;
using FTIR.Slicers;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using Shokouki.Configs;
using Shokouki.Presenters;

namespace Shokouki.Controllers
{
    public class Injector
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
            return new IntelligentCrestFinder(
                config.RepetitionRate,
                SamplingConfigs.Get().SamplingRate,
                SliceConfigs.Get().PointsBeforeCrest,
                SliceConfigs.Get().CrestAmplitudeThreshold
                );
        }

        [NotNull]
        public static Accumulator NewAccumulator()
        {
            var threadNum = Configurations.Get().ThreadNum;
            var correctors = new List<ICorrector>(threadNum);
            for (var i = 0; i < threadNum; i++)
            {
                correctors.Add(NewCorrector());
            }
            return new ParallelAccumulator(
                NewSlicer(),
                correctors);
//                return new SequentialAccumulator(NewSlicer(),NewCorrector());
        }

        [NotNull]
        public static DisplayAdapter NewAdapter(IScopeView view)
        {
            return new DisplayAdapter(view, Configurations.Get().DispPoints, SamplingConfigs.Get().SamplingRate);
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
        public static ICorrector NewCorrector()
        {
            var configs = Configurations.Get();
            var fuzzyPeriodLength = (int) (SamplingConfigs.Get().SamplingRate/configs.RepetitionRate);
            return new AccFlipMertzCorrector(NewApodizer(), fuzzyPeriodLength, configs.ZeroFillFactor,
                configs.CentreSpanLength/2);
        }

        [NotNull]
        public static IApodizer NewApodizer()
        {
            return new TriangulerApodizer();
        }
    }
}