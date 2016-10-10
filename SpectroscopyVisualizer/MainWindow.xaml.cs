using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Restricted;
using PhaseSonar.Analyzers;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Factories;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;

namespace SpectroscopyVisualizer {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml, also the entrance of the whole program.
    /// </summary>
    public partial class MainWindow : Window {
        private readonly CanvasView _canvasView;

        public MainWindow() {
            // init system components
            InitializeComponent();

            if (File.Exists(@"default.svcfg")) {
                ConfigsHolder.Load(@"default.svcfg");
            } else {
                // init configurations
                SamplingConfigurations.Initialize(
                    deviceName: "Dev2",
                    channel: 0,
                    samplingRateInMHz: 100,
                    recordLengthInM: 1,
                    range: 10);

                GeneralConfigurations.Initialize(
                    repetitionRate: 400,
                    threadNum: 4,
                    dispPoints: 1000,
                    directory: @"C:\buffer\captured\",
                    viewPhase: false);

                SliceConfigurations.Initialize(
                    crestAmplitudeThreshold: 1,
                    pointsBeforeCrest: 1000,
                    crestAtCenter: true,
                    rulerType: RulerType.MinLength,
                    findAbs: true,
                    autoAdjust: false,
                    fixedLength: 232171
                    );

                CorrectorConfigurations.Initialize(
                    zeroFillFactor: 1,
                    centerSpanLength: 512,
                    correctorType: CorrectorType.Mertz,
                    apodizerType: ApodizerType.Hann,
                    phaseType: PhaseType.FullRange,
                    autoFlip: false,
                    realPhase: false,
                    rangeStart: 18000,
                    rangeEnd: 20000);
            }
            //            CorrectorConfigs.Register(Toolbox.DeserializeData<CorrectorConfigs>(@"D:\\configuration.bin"));
            // bind configs to controls
            SamplingConfigurations.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            GeneralConfigurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath, CkPhase);
            SliceConfigurations.Get()
                .Bind(TbPtsBeforeCrest, TbCrestMinAmp, CbSliceLength, CkAutoAdjust, CkFindAbs, TbFixedLength);
            CorrectorConfigurations.Get()
                .Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType, CbPhaseType, TbRangeStart,
                    TbRangeEnd, CkAutoFlip, CkSpecReal);
            // init custom components
            _canvasView = new CanvasView(ScopeCanvas);
            HorizontalAxisView = new HorizontalAxisView(HorAxisCanvas);
            VerticalAxisView = new VerticalAxisView(VerAxisCanvas);

            CbPhaseType.SelectionChanged += (sender, args) => {
                var selected = (PhaseType) args.AddedItems[0];
                HideAllPhaseOptions();
                switch (selected) {
                    case PhaseType.FullRange:
                        break;
                    case PhaseType.CenterInterpolation:
                    case PhaseType.OldCenterInterpolation:
                        Show(TbCenterSpanLength);
                        Show(LbCentralSpan);
                        break;
                    case PhaseType.SpecifiedRange:
                    case PhaseType.SpecifiedFreqRange:
                        Show(LbRangeStart);
                        Show(LbRangeEnd);
                        Show(TbRangeStart);
                        Show(TbRangeEnd);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            CbSliceLength.SelectionChanged += (sender, args) => {
                var selected = (RulerType) args.AddedItems[0];
                TbFixedLength.Visibility = selected == RulerType.FixLength ? Visibility.Visible : Visibility.Hidden;
            };

            SwitchButton = new SwitchButton(ToggleButton, false, "STOP", "START", TurnOn, TurnOff);

//            Toolbox.SerializeData(@"D:\\configuration.bin",CorrectorConfigs.Get());
            SizeChanged += (sender, args) => { Adapter?.OnWindowZoomed(); };
            // todo text disapeared

            ConsoleManager.Show();
//            Logger.WriteLine("testing","testtest");
        }

        private SwitchButton SwitchButton { get; }

        [CanBeNull]
        public DisplayAdapter Adapter { get; set; }

        public HorizontalAxisView HorizontalAxisView { get; }
        public VerticalAxisView VerticalAxisView { get; }

        [NotNull]
        public IScheduler Scheduler { get; private set; } = new EmptyScheduler();


        private void HideAllPhaseOptions() {
            Hide(TbCenterSpanLength);
            Hide(LbCentralSpan);
            Hide(TbRangeStart);
            Hide(LbRangeStart);
            Hide(TbRangeEnd);
            Hide(LbRangeEnd);
        }

        private static void Hide(Control control) {
            control.Visibility = Visibility.Hidden;
        }

        private static void Show(Control control) {
            control.Visibility = Visibility.Visible;
        }


        private static bool IsChecked(ToggleButton checkBox) {
            return checkBox.IsChecked != null && checkBox.IsChecked.Value;
        }

        private bool DeveloperMode() {
            return TbChannel.Text == "256";
        }

        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs) {
            SwitchButton.Toggle();
        }

        private void TurnOff() {
            Scheduler.Stop();
        }

        private void AttachWriter(IProducerV2<SampleRecord> producer) {
            if (IsChecked(CbCaptureSample)) {
                var newSampleWriter = FactoryHolder.Get().NewSampleWriter();
                producer.NewProduct += record => { newSampleWriter.Write(record); };
            }
        }

        private void TurnOn() {
            GC.Collect();
            var factory = FactoryHolder.Get();
            var producer = factory.NewProducer();
            AttachWriter(producer);
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);
            var newSpectrumWriter = IsChecked(CbCaptureSpec) ? factory.NewSpectrumWriter() : null;
            var consumer = factory.NewConsumer(producer, Adapter, newSpectrumWriter, null);
            try {
                Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
            } catch (Exception) {
            }
            Scheduler = new Scheduler(producer, consumer);
            consumer.SourceInvalid += ConsumerOnFailEvent;
            consumer.ElementConsumedSuccessfully += () => { ConsumerOnConsumeEvent(consumer, Scheduler.Watch); };
            TbStartFreq.DataContext = Adapter;
            TbEndFreq.DataContext = Adapter;
            Scheduler.Start();
        }

        private void ConsumerOnConsumeEvent(IConsumerV2 consumer, StopWatch watch) {
            var sizeInM = SamplingConfigurations.Get().RecordLength/1e6;
            Application.Current.Dispatcher.InvokeAsync(() => {
                var consumedCnt = consumer.ConsumedCnt;
                var elapsedSeconds = watch.ElapsedSeconds();
                var speed = consumedCnt*sizeInM/elapsedSeconds;
                TbConsumerSpeed.Text = speed.ToString("F3");
                TbTotalData.Text = (consumedCnt*sizeInM).ToString();
                PbLoading.Value += 1;
            });
        }


        private void ConsumerOnFailEvent() {
            Scheduler.Stop();
            MessageBox.Show("It seems that the source is invalid.");
            Application.Current.Dispatcher.InvokeAsync(() => { SwitchButton.Toggle(false); });
        }


        private void BnPath_Click(object sender, RoutedEventArgs e) {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok) {
                foreach (var fileName in dialog.FileNames) {
                    TbSavePath.Text = fileName;
                }
            }
            GeneralConfigurations.Get().Directory = TbSavePath.Text;
        }


        private void DecodeFiles_OnClick(object sender, RoutedEventArgs e) {
            var files = SelectFiles();
            PbLoading.Maximum = files.Length;
            PbLoading.Value = 0;
            if (!files.IsEmpty()) {
                Task.Run(() => {
                    files.ForEach(path => {
                        var deserializeData = Toolbox.DeserializeData<double[]>(path);
                        Toolbox.WriteData(path.Replace("Binary", "Decoded"), deserializeData);
                        PbLoading.Dispatcher.InvokeAsync(() => { PbLoading.Value += 1; });
                    });
                    MessageBox.Show("decoding finished");
                });
            }
        }

        private static string[] SelectFiles() {
            // Create OpenFileDialog 
            var dlg = new OpenFileDialog {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt",
                Multiselect = true
            };

            // Set filter for file extension and default file extension 


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();
            if (result == true) {
                return dlg.FileNames;
            }
            return new string[0];
        }

        private void LoadCompressedFiles_OnClick(object sender, RoutedEventArgs e) {
            LoadFiles(true);
        }

        private void LoadFiles(bool compressed) {
            var fileNames = SelectFiles();
            if (fileNames.IsEmpty()) return;
            GeneralConfigurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
            TbSavePath.Text = GeneralConfigurations.Get().Directory;
            var factory = FactoryHolder.Get();
            var producer = factory.NewProducer(fileNames, compressed);
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);
            var newSpectrumWriter = IsChecked(CbCaptureSpec) ? factory.NewSpectrumWriter() : null;
            var consumer = factory.NewConsumer(producer, Adapter, newSpectrumWriter, fileNames.Length);
            consumer.SourceInvalid += ConsumerOnFailEvent;
            consumer.ElementConsumedSuccessfully += () => { ConsumerOnConsumeEvent(consumer, Scheduler.Watch); };
            consumer.ProducerEmpty += OnConsumerStopped;
            consumer.TargetAmountReached += OnConsumerStopped;
            CbCaptureSpec.IsChecked = !GeneralConfigurations.Get().ViewPhase;
            PbLoading.Maximum = fileNames.Length;
            PbLoading.Value = 0;
            Scheduler = new Scheduler(producer, consumer);
            Scheduler?.Start();
        }

        private void OnConsumerStopped() {
            Scheduler.Stop();
            MessageBox.Show("processing finished");
        }

        private void About_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("A 2016 ST Workshop Production. All Rights Reserved.");
        }

        private void StartSample_OnClick(object sender, RoutedEventArgs e) {
            var total = int.Parse(TbTotalData.Text);
            var factory = FactoryHolder.Get();

            var producer = factory.NewProducer(total);
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var workers = new List<SpecialSampleWriter>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                workers.Add(new SpecialSampleWriter(GeneralConfigurations.Get().Directory, "[Binary]"));
            }
            var consumer = new DataSerializer(producer.BlockingQueue, workers, total);
            consumer.SourceInvalid += ConsumerOnFailEvent;
            consumer.ElementConsumedSuccessfully += () => { ConsumerOnConsumeEvent(consumer, Scheduler.Watch); };
            consumer.TargetAmountReached += () => {
                Scheduler.Stop();
                MessageBox.Show("processing finished");
            };
            Scheduler = new Scheduler(producer, consumer);
            Scheduler?.Start();
        }

        private void LoadDataFiles_OnClick(object sender, RoutedEventArgs e) {
            LoadFiles(false);
        }

        private void DebugCmd_OnClick(object sender, RoutedEventArgs e) {
            var fileNames = SelectFiles();
            if (fileNames.IsEmpty()) return;

            GeneralConfigurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
            TbSavePath.Text = GeneralConfigurations.Get().Directory;
            var factory = FactoryHolder.Get();
            var producer = factory.NewProducer(fileNames, true);
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);

            var checkers = new List<PulseChecker>();
            for (var i = 0; i < 4; i++) {
                checkers.Add(new PulseChecker(factory.NewCrestFinder(), factory.NewSlicer(),
                    factory.NewPulsePreprocessor(), factory.NewCorrector()));
            }
            var consumer = new PulseByPulseChecker(producer.BlockingQueue, checkers, fileNames.Length);
            consumer.ElementConsumedSuccessfully +=
                () => { PbLoading.Dispatcher.InvokeAsync(() => { PbLoading.Value += 1; }); };
            PbLoading.Maximum = fileNames.Length;
            PbLoading.Value = 0;
            Scheduler = new Scheduler(producer, consumer);
            Scheduler.Start();
        }

        private void LoadConfig_OnClick(object sender, RoutedEventArgs e) {
            ConfigsHolder.Load();
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e) {
            new ConfigsHolder().Dump();
        }

        private void SetConfigAsDef_OnClick(object sender, RoutedEventArgs e) {
            new ConfigsHolder().Dump("default.svcfg");
        }
    }
}