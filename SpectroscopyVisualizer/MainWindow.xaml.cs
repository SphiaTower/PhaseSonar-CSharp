using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
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

        private bool _ultraFastMode;

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
                    viewPhase: false,
                    saveType: SaveType.Magnitude);

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
            GeneralConfigurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath, CkPhase, CbSaveType);
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

            SwitchButton = new ToggleButtonV2(ToggleButton, false, "STOP", "START");
            SwitchButton.TurnOn += TurnOn;
            SwitchButton.TurnOff += () => { Scheduler.Stop(); };
            SizeChanged += (sender, args) => { Adapter?.OnWindowZoomed(); };
            // todo text disapeared
        }

        private ToggleButtonV2 SwitchButton { get; }

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


        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs) {
            SwitchButton.State = !SwitchButton.State;
        }

        private void AttachWriter(IProducerV2<SampleRecord> producer) {
            if (IsChecked(CbCaptureSample)) {
                var newSampleWriter = FactoryHolder.Get().NewSampleWriter();
                producer.NewProduct += record => { newSampleWriter.Write(record); };
            }
        }

        private void TurnOn() {
            GC.Collect();
            if (_ultraFastMode) {
                var textBlock = new TextBlock {
                    Text = "Happy 2016!",
                    Foreground = new SolidColorBrush(Colors.Wheat),
                    FontSize = 30
                };
                Canvas.SetTop(textBlock, _canvasView.ScopeHeight/2);
                Canvas.SetLeft(textBlock, _canvasView.ScopeWidth/3);
                _canvasView.Canvas.Children.Add(textBlock);
                SwitchButton.State = false;
                return;
            }

            var factory = FactoryHolder.Get();
            IProducerV2<SampleRecord> producer;
            if (!factory.TryNewSampleProducer(out producer)) {
                SwitchButton.State = false;
                MessageBox.Show("Sampler can't be initialized");
                return;
            }
            producer.ProductionFailed += () => {
                Dispatcher.InvokeAsync(() => {
                    SwitchButton.State = false;
                    MessageBox.Show("Unable to sample data.");
                });
            };
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
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
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


        private void ConsumerOnSourceInvalid() {
            Scheduler.Stop();
            MessageBox.Show("It seems that the source is invalid.");
            Application.Current.Dispatcher.InvokeAsync(() => { SwitchButton.State = false; });
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

        private bool IsProgramRunning() {
            if (SwitchButton.State) {
                MessageBox.Show("Command rejected! Plz terminate current processing first.");
            }
            return SwitchButton.State;
        }

        private void LoadFiles(bool compressed) {
            if (IsProgramRunning()) {
                return;
            }
            var fileNames = SelectFiles();
            if (fileNames.IsEmpty()) return;
            GeneralConfigurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
            TbSavePath.Text = GeneralConfigurations.Get().Directory;
            var factory = FactoryHolder.Get();
            var producer = factory.NewProducer(fileNames, compressed);
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);
            CbCaptureSpec.IsChecked = !GeneralConfigurations.Get().ViewPhase;
            var newSpectrumWriter = IsChecked(CbCaptureSpec) ? factory.NewSpectrumWriter() : null;
            var consumer = factory.NewConsumer(producer, Adapter, newSpectrumWriter, fileNames.Length);
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
            consumer.ElementConsumedSuccessfully += () => { ConsumerOnConsumeEvent(consumer, Scheduler.Watch); };
            consumer.ProducerEmpty += OnConsumerStopped;
            consumer.TargetAmountReached += OnConsumerStopped;
            PbLoading.Maximum = fileNames.Length;
            PbLoading.Value = 0;
            Scheduler = new Scheduler(producer, consumer);
            Scheduler.Start();
            SetButtonRunning();
        }

        private void OnConsumerStopped() {
            Dispatcher.InvokeAsync(() => { SwitchButton.State = false; });
        }

        private void About_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("A 2016 ST Workshop Production. All Rights Reserved.");
        }

        private void StartSample_OnClick(object sender, RoutedEventArgs e) {
            if (IsProgramRunning()) {
                return;
            }
            var total = int.Parse(TbTotalData.Text);
            var factory = FactoryHolder.Get();

            IProducerV2<SampleRecord> producer;
            if (!factory.TryNewSampleProducer(out producer)) {
                MessageBox.Show("Sampler can't be initialized");
                return;
            }
            Adapter = factory.NewAdapter(_canvasView, HorizontalAxisView, VerticalAxisView, TbXCoordinate, TbDistance);
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var workers = new List<SpecialSampleWriter>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                workers.Add(new SpecialSampleWriter(GeneralConfigurations.Get().Directory, "[Binary]"));
            }
            var consumer = new DataSerializer(producer.BlockingQueue, workers, total);
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
            consumer.ElementConsumedSuccessfully += () => { ConsumerOnConsumeEvent(consumer, Scheduler.Watch); };
            consumer.TargetAmountReached += () => {
                Scheduler.Stop();
                // MessageBox.Show("processing finished");
            };
            Scheduler = new Scheduler(producer, consumer);
            Scheduler.Start();
            SetButtonRunning();
        }

        private void SetButtonRunning() {
            SwitchButton.TurnOn -= TurnOn;
            SwitchButton.State = true;
            SwitchButton.TurnOn += TurnOn;
        }

        private void LoadDataFiles_OnClick(object sender, RoutedEventArgs e) {
            LoadFiles(false);
        }

        private void DebugCmd_OnClick(object sender, RoutedEventArgs e) {
            if (IsProgramRunning()) {
                return;
            }
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
            SetButtonRunning();
        }

        private void LoadConfig_OnClick(object sender, RoutedEventArgs e) {
            ConfigsHolder.Load();
        }

        private void SaveConfig_OnClick(object sender, RoutedEventArgs e) {
            new ConfigsHolder().Dump();
        }

        private void SetConfigAsDef_OnClick(object sender, RoutedEventArgs e) {
            new ConfigsHolder().Dump("default.svcfg");
            MessageBox.Show("Default configuration set successfully");
        }

        private void Donate_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show(
                "If you think this app is valuable, plz pay $10 USD to the author. \n\nYour support is very important! Thanks!");
        }

        private void ReportBug_OnClick(object sender, RoutedEventArgs e) {
            var issue = @"https://github.com/SphiaTower/PhaseSonar-CSharp/issues";
            Process.Start(issue);
        }

        private void ContactAuthor_OnClick(object sender, RoutedEventArgs e) {
        
        }

        private void UltraFast_OnChecked(object sender, RoutedEventArgs e) {
            _ultraFastMode = CkUltraFast.IsChecked;
        }

        private void CkUltraFast_OnUnchecked(object sender, RoutedEventArgs e) {
            _ultraFastMode = CkUltraFast.IsChecked;
        }
    }
}