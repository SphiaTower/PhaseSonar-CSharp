using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Examples.StreamToDiskConsole;
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
        private const int ProgressIndeterminable = 12345;
        private readonly Brush _originalBrush;

        private readonly Thickness _originalThickness;

        private readonly Label[] _possibleWrongLabels;

        [CanBeNull] private StatsWindow _statsWindow = new StatsWindow();

        //        private readonly CanvasView _canvasView;

        private bool _ultraFastMode;

        public SliceConfigurations SliceConfigs { get; set; }

        public GeneralConfigurations GeneralConfigs { get; set; }

        public SamplingConfigurations SampleConfigs { get; set; }

        public CorrectorConfigurations CorrectorConfigs { get; set; }

        public MainWindow() {
            // init system components
            InitializeComponent();

            _possibleWrongLabels = new[] {LbPeakMinAmp, LbRangeStart, LbRangeEnd, LbRepRate};
            _originalThickness = _possibleWrongLabels[0].BorderThickness;
            _originalBrush = _possibleWrongLabels[0].BorderBrush;

            if (File.Exists(@"default.svcfg")) {
                ConfigsHolder.Load(@"default.svcfg");
            } else {
                // init configurations
                SamplingConfigurations.Initialize(
                    deviceName: "Dev3",
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
                    saveType: SaveType.Magnitude,
                    queueSize: 48,
                    saveSample: false,
                    saveSpec: false,
                    saveAcc: false,
                    operationMode: OperationMode.Manual,
                    targetCnt: 100);

                SliceConfigurations.Initialize(
                    crestAmplitudeThreshold: 0.5,
                    peakMinLength: 2000,
                    crestAtCenter: true,
                    rulerType: RulerType.MinLength,
                    findAbs: true,
                    autoAdjust: true,
                    fixedLength: 232171,
                    reference: false
                    );

                CorrectorConfigurations.Initialize(
                    zeroFillFactor: 1,
                    centerSpanLength: 512,
                    correctorType: CorrectorType.Mertz,
                    apodizerType: ApodizerType.Hann,
                    phaseType: PhaseType.FullRange,
                    autoFlip: false,
                    realPhase: false,
                    rangeStart: 3,
                    rangeEnd: 4);

                MiscellaneousConfigurations.Initialize(
                    waitEmptyProducerMsTimeout: 5000,
                    minFlatPhasePtsNumCnt: 200,
                    maxPhaseStd: 0.34,
                    pythonPath: @"C:\Anaconda3\python.exe");
            }


            SliceConfigs = SliceConfigurations.Get();
            CorrectorConfigs = CorrectorConfigurations.Get();
            SampleConfigs = SamplingConfigurations.Get();
            GeneralConfigs = GeneralConfigurations.Get();

            CbPhaseType.SelectionChanged += (sender, args) => {
                HideAllPhaseOptions();

                var selected = (PhaseType) args.AddedItems[0];
                HandleAdditionalPhaseOptions(selected);
            };

            CbSliceLength.SelectionChanged += (sender, args) => {
                var selected = (RulerType) args.AddedItems[0];
                TbFixedLength.Visibility = selected == RulerType.FixLength ? Visibility.Visible : Visibility.Hidden;
            };

            CbCorrector.SelectionChanged += (sender, args) => {
                var selected = (CorrectorType) args.AddedItems[0];
                switch (selected) {
                    case CorrectorType.Fake:
                        Hide(CbPhaseType);
                        Hide(LbPhaseType);
                        HideAllPhaseOptions();
                        break;
                    case CorrectorType.Mertz:
                        Show(CbPhaseType);
                        Show(LbPhaseType);
                        HandleAdditionalPhaseOptions((PhaseType?) CbPhaseType.SelectedItem ?? PhaseType.FullRange);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
            CbOperationMode.SelectionChanged += (sender, args) => {
                var selected = (OperationMode) args.AddedItems[0];
                switch (selected) {
                    case OperationMode.Manual:
                        Hide(TbTargetCnt);
                        break;
                    case OperationMode.Single:
                    case OperationMode.Loop:
                        Show(TbTargetCnt);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
            RoutedEventHandler ckPhaseOnChecked = (sender, args) => {
                var correct = !CkPhase.IsChecked.GetValueOrDefault(false);
                if (correct) {
                    Show(LbCorrector);
                    Show(CbCorrector);
                    Show(CbApodizationType);
                    Show(LbApodize);
                    Show(CkAutoFlip);
                    Show(CkSpecReal);
                    if ((CorrectorType) CbCorrector.SelectedItem != CorrectorType.Fake) {
                        Show(CbPhaseType);
                        Show(LbPhaseType);
                        HandleAdditionalPhaseOptions((PhaseType?) CbPhaseType.SelectedItem ?? PhaseType.FullRange);
                    }
                } else {
                    Hide(CbCorrector);
                    Hide(LbCorrector);
                    Hide(CbApodizationType);
                    Hide(LbApodize);
                    Hide(CkAutoFlip);
                    Hide(CkSpecReal);
                    Hide(CbPhaseType);
                    Hide(LbPhaseType);
                    HideAllPhaseOptions();
                }
            };
            CkPhase.Checked += ckPhaseOnChecked;
            CkPhase.Unchecked += ckPhaseOnChecked;

            // bind configs to controls
            DataContext = this;

//            SamplingConfigurations.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
//            GeneralConfigurations.Get()
//                .Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath, CkPhase, CbSaveType, TbQueueSize,
//                    CkCaptureSample, CkCaptureSpec, CkCaptureAcc, CbOperationMode, TbTargetCnt);
//            SliceConfigurations.Get()
//                .Bind(TbPtsBeforeCrest, TbCrestMinAmp, CbSliceLength, CkAutoAdjust, CkFindAbs, TbFixedLength, CkRef);
//            CorrectorConfigurations.Get()
//                .Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType, CbPhaseType, TbRangeStart,
//                    TbRangeEnd, CkAutoFlip, CkSpecReal);
            // init custom components

            SwitchButton = new ToggleButtonV2(ToggleButton, false, "Stop", "Start");
            SwitchButton.TurnOn += TurnOn;
            SwitchButton.TurnOff += ClearFromRunningState;
            SizeChanged += (sender, args) => { Adapter?.OnWindowZoomed(); };
            // todo text disapeared

            CkCaptureSpec.Checked += (sender, args) => { CkCaptureAcc.IsChecked = true; };

            Closing += (sender, args) => { _statsWindow?.Close(); };

            LocationChanged += (sender, args) => {
                if (_statsWindow==null) {
                    return;
                }
                _statsWindow.Left = this.Left + this.Width - 15;
                _statsWindow.Top = this.Top;
            };
          
        }


        private ToggleButtonV2 SwitchButton { get; }

        [CanBeNull]
        public DisplayAdapterV2 Adapter { get; set; }

        [NotNull]
        public IScheduler Scheduler { get; private set; } = new EmptyScheduler();

        private void HandleAdditionalPhaseOptions(PhaseType selected) {
            switch (selected) {
                case PhaseType.FullRange:
                    break;
                case PhaseType.CenterInterpolation:
                case PhaseType.OldCenterInterpolation:
                    Show(TbCenterSpanLength);
                    Show(LbCentralSpan);
                    break;
                case PhaseType.SpecificRange:
                case PhaseType.SpecificFreqRange:
                    Show(LbRangeStart);
                    Show(LbRangeEnd);
                    Show(TbRangeStart);
                    Show(TbRangeEnd);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


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


        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs) {
            SwitchButton.State = !SwitchButton.State;
        }

        private void TurnOn() {
            GC.Collect();

            if (_ultraFastMode) {
                var textBlock = new TextBlock {
                    Text = "Happy 2016!",
                    Foreground = new SolidColorBrush(Colors.Wheat),
                    FontSize = 30
                };
                var canvasView = new CanvasView(ScopeCanvas);
                Canvas.SetTop(textBlock, canvasView.ScopeHeight/2);
                Canvas.SetLeft(textBlock, canvasView.ScopeWidth/3);
                canvasView.Canvas.Children.Add(textBlock);
                SwitchButton.State = false;
                return;
            }

            var factory = FactoryHolder.Get();
            IProducerV2<SampleRecord> producer;
            if (!factory.TryNewSampleProducer(out producer)) {
                SwitchButton.State = false;
                MessageBox.Show("Sampler can't be initialized. Maybe another instance is running.");
                return;
            }
            producer.ProductionFailed += exception => {
                Dispatcher.InvokeAsync(() => {
                    SwitchButton.State = false;
                    MessageBox.Show("Unable to sample data. Exceptions occured, please read carefully:\n\n" +
                                    exception.Message);
                });
            };
            Adapter = NewAdapter();
            int? targetCnt;
            var configs = GeneralConfigurations.Get();
            if (configs.OperationMode == OperationMode.Manual) {
                targetCnt = null;
            } else {
                targetCnt = configs.TargetCnt;
            }
            var consumer = factory.NewConsumer(producer, Adapter, targetCnt);
            try {
                Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
            } catch (Exception) {
            }
            Scheduler = new Scheduler(producer, consumer);
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
            consumer.Update += ConsumerOnUpdate;
            switch (configs.OperationMode) {
                case OperationMode.Manual:
                    PbLoading.IsIndeterminate = true;

                    break;
                case OperationMode.Single:
                    consumer.TargetAmountReached += OnConsumerStopped;
                    PbLoading.IsIndeterminate = false;
                    PbLoading.Maximum = targetCnt.Value;
                    break;
                case OperationMode.Loop:
                    PbLoading.IsIndeterminate = false;
                    PbLoading.Maximum = targetCnt.Value;
                    consumer.TargetAmountReached += () => {
                        OnConsumerStopped();

                        Dispatcher.Invoke(() => { SwitchButton.State = true; });
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TbStartFreq.DataContext = Adapter;
            TbEndFreq.DataContext = Adapter;
            PrepareStatsWinndow();
            Scheduler.Start();
        }

        private void PrepareStatsWinndow() {
            if (_statsWindow == null) {
                _statsWindow = new StatsWindow();
            }
            if (!_statsWindow.IsVisible) {
                _statsWindow.Left = Left + Width - 15;
                _statsWindow.Top = Top;
                _statsWindow.Show();
                _statsWindow.Closed += (sender, args) => { _statsWindow = null; };
            }
            _statsWindow.Reset();

            foreach (var label in _possibleWrongLabels) {
                label.BorderThickness = _originalThickness;
                label.BorderBrush = _originalBrush;
            }
        }

        private void ConsumerOnUpdate(IResult result) {
            _statsWindow.Dispatcher.InvokeAsync(() => {
                _statsWindow.Time = Scheduler.Watch.ElapsedSeconds();
                _statsWindow.Update(result);
                if (PbLoading.Maximum != ProgressIndeterminable) {
                    PbLoading.Value += 1;
                }
            });
        }

        [NotNull]
        private DisplayAdapterV2 NewAdapter() {
            if (GeneralConfigurations.Get().ViewPhase) {
                return FactoryHolder.Get()
                    .NewPhaseAdapter(new CanvasView(ScopeCanvas), new HorizontalAxisView(HorAxisCanvas),
                        new VerticalAxisView(VerAxisCanvas), TbXCoordinate, TbDistance);
            }
            return FactoryHolder.Get()
                .NewSpectrumAdapter(new CanvasView(ScopeCanvas), new HorizontalAxisView(HorAxisCanvas),
                    new VerticalAxisView(VerAxisCanvas), TbXCoordinate, TbDistance);
        }

        private void ConsumerOnSourceInvalid() {
            Dispatcher.Invoke(() => {
                SwitchButton.State = false;
                var dispatcherTimer = new DispatcherTimer();
                var cnt = 0;
                dispatcherTimer.Tick += (sender, args) => {
                    if (cnt%2 == 0) {
                        foreach (var label in _possibleWrongLabels) {
                            label.BorderThickness = new Thickness(1.5);
                            label.BorderBrush = Brushes.Red;
                        }
                    } else {
                        foreach (var label in _possibleWrongLabels) {
                            label.BorderThickness = _originalThickness;
                            label.BorderBrush = _originalBrush;
                        }
                    }
                    cnt++;
                    if (cnt == 15) {
                        dispatcherTimer.Stop();
                    }
                };
                dispatcherTimer.Interval = TimeSpan.FromSeconds(0.4);
                dispatcherTimer.Start();
            });

            MessageBox.Show(
                "Data analysis failed. Please check\n\n   the INPUT signal,\n   PEAK MIN AMP,\n   REP FREQ DIFF\n   or PHASE RANGE.\n\nDetailed information is listed on the stats board.");
        }

        private void OnConsumerStopped() {
            Dispatcher.Invoke(() => { SwitchButton.State = false; });
        }

        private void ClearFromRunningState() {
            Scheduler.Stop();
            PbLoading.IsIndeterminate = false;
            if (PbLoading.Value < PbLoading.Maximum) {
                PbLoading.Value += 1;
            }
            PbLoading.Value = 0;
            PbLoading.Maximum = ProgressIndeterminable;
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

        private static string SelectFile() {
            // Create OpenFileDialog 
            var dlg = new OpenFileDialog {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt",
                Multiselect = false
            };

            // Set filter for file extension and default file extension 


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();
            if (result == true) {
                return dlg.FileNames[0];
            }
            return null;
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
            Adapter = NewAdapter();
            var consumer = factory.NewConsumer(producer, Adapter, fileNames.Length);
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
            consumer.Update += ConsumerOnUpdate;
            consumer.ProducerEmpty += OnConsumerStopped;
            consumer.TargetAmountReached += OnConsumerStopped;
            PbLoading.Maximum = fileNames.Length;
            PbLoading.Value = 0;
            Scheduler = new Scheduler(producer, consumer);
            SetButtonRunning();
            Scheduler.Start();
        }

        private void About_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("A 2016 ST Workshop Production. All Rights Reserved.");
        }

        private void StartSample_OnClick(object sender, RoutedEventArgs e) {
            if (IsProgramRunning()) {
                return;
            }
            var numberDialog = new NumberDialog();
            if (!numberDialog.ShowDialog().GetValueOrDefault(false)) {
                return;
            }
            var total = numberDialog.Number;
            if (total <= 0) {
                MessageBox.Show("plz input the number of records to be sampled.");
                return;
            }

            var factory = FactoryHolder.Get();

            IProducerV2<SampleRecord> producer;
            if (!factory.TryNewSampleProducer(out producer, total)) {
                SwitchButton.State = false;
                MessageBox.Show("Sampler can't be initialized");
                return;
            }
            producer.ProductionFailed += exception => {
                Dispatcher.InvokeAsync(() => {
                    SwitchButton.State = false;
                    MessageBox.Show("Unable to sample data. Exception occured:\n\n" + exception.Message);
                });
            };
            //            producer.HitTarget += () => { Dispatcher.InvokeAsync(() => { SwitchButton.State = false; }); };
            PbLoading.Maximum = total;
            Adapter = NewAdapter();
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var workers = new List<SpecialSampleWriter>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                workers.Add(new SpecialSampleWriter(GeneralConfigurations.Get().Directory, "[Binary]"));
            }
            var consumer = new DataSerializer(producer.BlockingQueue, workers, total);
            consumer.SourceInvalid += ConsumerOnSourceInvalid;
            consumer.Update += ConsumerOnUpdate;
            consumer.TargetAmountReached += () => { Dispatcher.InvokeAsync(() => { SwitchButton.State = false; }); };
            CkCaptureSample.IsChecked = true;
            Scheduler = new Scheduler(producer, consumer);
            SetButtonRunning();
            Scheduler.Start();
        }

        private void SetButtonRunning() {
            PrepareStatsWinndow();
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
            Adapter = NewAdapter();

            var checkers = new List<PulseChecker>();
            for (var i = 0; i < 4; i++) {
                checkers.Add(new PulseChecker(factory.NewCrestFinder(), factory.NewSlicer(),
                    factory.NewPulsePreprocessor(), factory.NewCorrector()));
            }
            var consumer = new PulseByPulseChecker(producer.BlockingQueue, checkers, fileNames.Length);
            consumer.Update += ConsumerOnUpdate;
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
            MessageBox.Show("Sorry for encountering a bug, but I won't fix it unless payed for.");
        }

        private void ContactAuthor_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("Q&A is free. Additional coding or document supports are only available to paying customers.");
            var address = @"";
            Process.Start(address);
        }

        private void UltraFast_OnChecked(object sender, RoutedEventArgs e) {
            _ultraFastMode = CkUltraFast.IsChecked;
        }

        private void CkUltraFast_OnUnchecked(object sender, RoutedEventArgs e) {
            _ultraFastMode = CkUltraFast.IsChecked;
        }

        private void GenerateWavelengthAxis_OnClick(object sender, RoutedEventArgs e) {
            if (!CheckPythonAndWarn()) {
                return;
            }
            var file = SelectFile();
            if (file != null) {
                var command = Quote(AppDomain.CurrentDomain.BaseDirectory + @"Pythons\Mapper.py") + " " + file+" "+SamplingConfigurations.Get().SamplingRate;
                Process.Start(MiscellaneousConfigurations.Get().PythonPath, command);
            }
        }

        public static bool CheckPython() {
            var pythonPath = MiscellaneousConfigurations.Get().PythonPath;
            return File.Exists(pythonPath);
        }

        public static bool CheckPythonAndWarn() {
            var checkPython = CheckPython();
            if (!checkPython) {
                MessageBox.Show("Unable to execute Python scripts, please set the path of Python interpreter in Tools->Miscellaneous Options.");
            }
            return checkPython;
        }

        [NotNull]
        private static string Quote(string str) {
            return "\"" + str + "\"";
        }

        private void FlattenCurves_OnClick(object sender, RoutedEventArgs e) {
            if (!CheckPythonAndWarn()) {
                return;
            }
            var file = SelectFile();
            if (file == null) {
                return;
            }
            var pythonPath = MiscellaneousConfigurations.Get().PythonPath;
            var cmd = Quote(AppDomain.CurrentDomain.BaseDirectory + @"Pythons\Flatter.py") + " " + file;
            if (File.Exists(file.Replace(".txt", "[WavelengthAxis].txt"))) {
                Process.Start(pythonPath, cmd);
            } else {
                var command = Quote(AppDomain.CurrentDomain.BaseDirectory + @"Pythons\Mapper.py") + " " + file + " " + SamplingConfigurations.Get().SamplingRate;
                Process.Start(pythonPath, command);
                MessageBox.Show("Generating wavelength axis, please click 'OK' AFTER completion.");
                Process.Start(pythonPath, cmd);
            }
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void BnOpenPath_OnClick(object sender, RoutedEventArgs e) {
            Process.Start(GeneralConfigurations.Get().Directory);
        }

        private void BnViewTemporal_OnClick(object sender, RoutedEventArgs e) {
            if (IsProgramRunning()) {
                return;
            }
            if (!CheckPythonAndWarn()) {
                return;
            }
            Task.Run(() => {
                var factory = new ParallelInjector();
                Sampler sampler;
                if (factory.TryNewSampler(out sampler)) {
                    var data = sampler.Retrieve(SamplingConfigurations.Get().Channel);
                    var finder = factory.NewCrestFinder();
                    var crestIndices = finder.Find(data);
                    var path = AppDomain.CurrentDomain.BaseDirectory + @"temporal\";
                    if (!Directory.Exists(path)) {
                        try {
                            Directory.CreateDirectory(path);
                        } catch (Exception) {
                            MessageBox.Show(
                                "Unable to create directory! Try run this program with administrator permission");
                            return;
                        }
                    }
                    Toolbox.WriteData(path + "temporal.txt", data);
                    Toolbox.WriteData(path + "crests.txt", crestIndices.ToArray());
                    Process.Start(MiscellaneousConfigurations.Get().PythonPath,
                        Quote(AppDomain.CurrentDomain.BaseDirectory + @"Pythons\TemporalViewer.py")+" "+ AppDomain.CurrentDomain.BaseDirectory);
                } else {
                    MessageBox.Show("Sampler can't be initialized. Maybe another instance is running.");
                }
            });
        }

        private void BnRestart_OnClick(object sender, RoutedEventArgs e) {
            SwitchButton.State = false;
            SwitchButton.State = true;
        }

        private void AdditionalOptions_OnClick(object sender, RoutedEventArgs e) {
            new OptionsWindow().Show();
        }

        private void CheckUpdate_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("No update available, never.");
        }

        private void Credits_OnClick(object sender, RoutedEventArgs e) {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            var textBlock = new TextBlock {
                Foreground = Brushes.White,
                FontSize = 30
            };
            ScopeCanvas.Children.Add(textBlock);
            Canvas.SetTop(textBlock,ScopeCanvas.ActualHeight/2);
            Canvas.SetLeft(textBlock,ScopeCanvas.ActualWidth / 3);
            string[] credits= {
            };
            int i = 0;
            bool show = true;
            dispatcherTimer.Tick += (o, args) => {
                if (i >= credits.Length) {
                    ScopeCanvas.Children.Remove(textBlock);
                    dispatcherTimer.Stop();
                } else {
                    if (show) {
                        textBlock.Text = credits[i++];
                        textBlock.Visibility = Visibility.Visible;
                    } else {
                        textBlock.Visibility = Visibility.Hidden;
                    }
                    show = !show;
                }

            };
            dispatcherTimer.Start();
        }

        private void ViewHelp_OnClick(object sender, RoutedEventArgs e) {
            Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"UserGuide.pdf");
        }
    }
}