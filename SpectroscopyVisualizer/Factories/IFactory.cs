using System.Collections.Generic;
using System.Windows.Controls;
using JetBrains.Annotations;
using NationalInstruments.Examples.StreamToDiskConsole;
using PhaseSonar.Analyzers;
using PhaseSonar.CorrectorV2s;
using PhaseSonar.CrestFinders;
using PhaseSonar.Slicers;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer.Factories {
    public interface IFactory {
        ISlicer NewSlicer();
        IRuler NewRuler();
        IAligner NewAligner();
        ICrestFinder NewCrestFinder();
        IPulsePreprocessor NewPulsePreprocessor();
        IPulseSequenceProcessor NewPulseSequenceProcessor();
        [NotNull]
        DisplayAdapter NewAdapter(CanvasView view, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView,TextBox tbX, TextBox tbDelta);

        bool TryNewSampler(out Sampler newSampler);
        IPhaseExtractor NewPhaseExtractor();
        ICorrectorV2 NewCorrector();
        bool TryNewSampleProducer(out IProducerV2<SampleRecord> newProducer,int? targetCnt=null);
        IProducerV2<SampleRecord> NewProducer(IReadOnlyCollection<string> paths, bool compressed);
        IWriterV2<TracedSpectrum> NewSpectrumWriter();
        IWriterV2<SampleRecord> NewSampleWriter();

        IConsumerV2 NewConsumer([NotNull] IProducerV2<SampleRecord> producer, [NotNull] DisplayAdapter adapter, IWriterV2<TracedSpectrum> writer, int? targetCnt);
    }
}