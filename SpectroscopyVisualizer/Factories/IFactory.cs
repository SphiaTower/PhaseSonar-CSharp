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

        Sampler NewSampler();
        IPhaseExtractor NewPhaseExtractor();
        ICorrectorV2 NewCorrectorNoFlip();
        ICorrectorV2 NewCorrector();
        SampleProducer NewProducer(bool cameraOn);
        DiskProducer NewProducer(IEnumerable<string> paths, bool compressed);
        SpectrumWriter NewSpectrumWriter(bool on);
        SampleWriter NewSampleWriter(bool on);

        AbstractConsumer<SampleRecord> NewConsumer([NotNull] IProducer<SampleRecord> producer,
            [NotNull] DisplayAdapter adapter, SpectrumWriter writer);
    }
}