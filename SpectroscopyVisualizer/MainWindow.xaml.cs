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
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;
using SpectroscopyVisualizer.Factories;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml, also the entrance of the whole program.
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            SamplingConfigurations.Initialize(
                "Dev2",
                0,
                100,
                1,
                10);

            GeneralConfigurations.Initialize(
                400,
                4,
                1000,
                @"C:\buffer\captured\");
            SliceConfigurations.Initialize(
                crestAmplitudeThreshold: 0.1,
                pointsBeforeCrest: 1000,
                crestAtCenter: true,
                rulerType: RulerType.MinLength
                );
            CorrectorConfigurations.Initialize(1, 512, CorrectorType.Mertz, ApodizerType.Fake, PhaseType.FullRange,
                20000, 24000);
//            CorrectorConfigs.Register(Toolbox.DeserializeData<CorrectorConfigs>(@"D:\\configuration.bin"));
            SamplingConfigurations.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            GeneralConfigurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath);
            SliceConfigurations.Get().Bind(TbPtsBeforeCrest, TbCrestMinAmp, CbSliceLength);
            CorrectorConfigurations.Get()
                .Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType, CbPhaseType, TbRangeStart,
                    TbRangeEnd);
            CanvasView = new CanvasView(ScopeCanvas);
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
                        Show(LbRangeStart);
                        Show(LbRangeEnd);
                        Show(TbRangeStart);
                        Show(TbRangeEnd);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            SwitchButton = new SwitchButton(ToggleButton, false, "STOP", "START", TurnOn, TurnOff);

//            Toolbox.SerializeData(@"D:\\configuration.bin",CorrectorConfigs.Get());
            SizeChanged += (sender, args) => { Adapter?.OnWindowZoomed(); };

            ConsoleManager.Show();
//            Logger.WriteLine("testing","testtest");
        }

        private SwitchButton SwitchButton { get; }

        [CanBeNull]
        public DisplayAdapter Adapter { get; set; }

        public CanvasView CanvasView { get; }
        public HorizontalAxisView HorizontalAxisView { get; }
        public VerticalAxisView VerticalAxisView { get; }

        [CanBeNull]
        public Scheduler Scheduler { get; private set; }

        [CanBeNull]
        public SpectrumWriter Writer { get; set; }

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
            Scheduler?.Stop();
        }

        private void TurnOn() {
            GC.Collect();
            IProducer<SampleRecord> producer;
            var factory = FactoryHolder.Get();
            if (DeveloperMode()) {
                producer = new DummyProducer();
            } else {
                producer = factory.NewProducer(IsChecked(CbCaptureSample));
            }
            Adapter = factory.NewAdapter(CanvasView, HorizontalAxisView, VerticalAxisView,TbXCoordinate,TbDistance);
            Writer = factory.NewSpectrumWriter(IsChecked(CbCaptureSpec));
            var consumer = factory.NewConsumer(producer, Adapter, Writer);
            try {
                Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
            } catch (Exception) {
            }
            Scheduler = new Scheduler(producer, consumer);
            consumer.FailEvent += ConsumerOnFailEvent;
            consumer.ConsumeEvent += ConsumerOnConsumeEvent;

            TbStartFreq.DataContext = Adapter;
            TbEndFreq.DataContext = Adapter;
            Scheduler.Start();
        }

        private void ConsumerOnConsumeEvent(object sender) {
            var sizeInM = SamplingConfigurations.Get().RecordLength/1e6;
            Application.Current.Dispatcher.InvokeAsync(() => {
                var consumedCnt = Scheduler?.Consumer.ConsumedCnt;
                var elapsedSeconds = Scheduler?.Watch.ElapsedSeconds();
                var speed = consumedCnt*sizeInM/elapsedSeconds;
                if (consumedCnt.HasValue) {
                    TbConsumerSpeed.Text = speed.Value.ToString("F3");
                    TbTotalData.Text = (consumedCnt.Value*sizeInM).ToString();
                }
            });
        }

        private void ConsumerOnFailEvent(object sender) {
            Scheduler?.Stop();
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


        private void OnCaptureSampleChecked(object sender, RoutedEventArgs e) {
            var producer = Scheduler?.Producer as SampleProducer;
            if (producer == null) return;
            if (CbCaptureSample.IsChecked != null) producer.Writer.IsOn = CbCaptureSample.IsChecked.Value;
        }

        private void CbCaptureSpec_OnChecked(object sender, RoutedEventArgs e) {
            if (Writer != null) Writer.IsOn = IsChecked(CbCaptureSpec);
        }

        private void DecodeFiles_OnClick(object sender, RoutedEventArgs e) {
            var files = SelectFiles();
            if (!files.IsEmpty()) {
                Task.Run(() => {
                    files.ForEach(path => {
                        var deserializeData = Toolbox.DeserializeData<double[]>(path);
                        Toolbox.WriteData(path.Replace("Binary", "Decoded"), deserializeData);
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
            Adapter = factory.NewAdapter(CanvasView, HorizontalAxisView, VerticalAxisView,TbXCoordinate,TbDistance);
            Writer = factory.NewSpectrumWriter(IsChecked(CbCaptureSpec));
            var consumer = factory.NewConsumer(producer, Adapter, Writer);
            consumer.FailEvent += ConsumerOnFailEvent;
            consumer.ConsumeEvent += ConsumerOnConsumeEvent;
            consumer.NoProductEvent += o => {
                Scheduler?.Stop();
                MessageBox.Show("processing finished");
                Scheduler = null;
//                Dispatcher.InvokeAsync(() => {
//                    ConsoleManager.Show();
//                    Console.WriteLine(Logger.Queue.Count);
//                    Logger.Queue.ForEach(str => {
//                        Console.WriteLine(str);
//                    });
//                });
            };
            CbCaptureSpec.IsChecked = true;
            Scheduler = new Scheduler(producer, consumer);
            Scheduler?.Start();
        }

        private void About_OnClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("A 2016 ST Workshop Production. All Rights Reserved.");
        }

        private void StartSample_OnClick(object sender, RoutedEventArgs e) {
            var total = int.Parse(TbTotalData.Text);
            var factory = FactoryHolder.Get();
            var producer = new FixedSampleProducer(factory.NewSampler(), total);
            Adapter = factory.NewAdapter(CanvasView, HorizontalAxisView, VerticalAxisView,TbXCoordinate,TbDistance);
            Writer = factory.NewSpectrumWriter(IsChecked(CbCaptureSpec));
            var threadNum = GeneralConfigurations.Get().ThreadNum;
            var workers = new List<SpecialSampleWriter>(threadNum);
            for (var i = 0; i < threadNum; i++) {
                workers.Add(new SpecialSampleWriter(GeneralConfigurations.Get().Directory, "[Binary]"));
            }
            var consumer = new DataSerializer(producer.BlockingQueue, workers);
            consumer.FailEvent += ConsumerOnFailEvent;
            consumer.ConsumeEvent += ConsumerOnConsumeEvent;
            consumer.ConsumeEvent += o => {
                if (consumer.ConsumedCnt >= total) {
                    Scheduler?.Stop();
                }
            };
            consumer.NoProductEvent += o => {
                Scheduler?.Stop();
                MessageBox.Show("processing finished");
                Scheduler = null;
            };
            Scheduler = new Scheduler(producer, consumer);
            Scheduler?.Start();
        }

        private void LoadDataFiles_OnClick(object sender, RoutedEventArgs e) {
            LoadFiles(false);
        }
    }
}