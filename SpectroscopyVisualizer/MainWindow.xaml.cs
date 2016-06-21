using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Restricted;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Factories;
using SpectroscopyVisualizer.Presenters;
using SpectroscopyVisualizer.Producers;
using SpectroscopyVisualizer.Writers;

namespace SpectroscopyVisualizer
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml, also the entrance of the whole program.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
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
                centreSlice: true
                );
            CorrectorConfigurations.Initialize(1, 512, CorrectorType.Mertz, ApodizerType.Fake);
//            CorrectorConfigs.Register(Toolbox.DeserializeData<CorrectorConfigs>(@"D:\\configuration.bin"));
            SamplingConfigurations.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            GeneralConfigurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath);
            SliceConfigurations.Get().Bind(TbPtsBeforeCrest, TbCrestMinAmp);
            CorrectorConfigurations.Get().Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType);
            CanvasView = new CanvasView(ScopeCanvas);
            HorizontalAxisView = new HorizontalAxisView(HorAxisCanvas);
            VerticalAxisView = new VerticalAxisView(VerAxisCanvas);


            SwitchButton = new SwitchButton(ToggleButton, false, "STOP", "START", TurnOn, TurnOff);

//            Toolbox.SerializeData(@"D:\\configuration.bin",CorrectorConfigs.Get());
            SizeChanged += (sender, args) => { Adapter?.OnWindowZoomed(); };
        }

        private SwitchButton SwitchButton { get; }
        [CanBeNull]
        public DisplayAdapter Adapter { get; set; }

        public CanvasView CanvasView { get; }
        public HorizontalAxisView HorizontalAxisView { get; }
        public VerticalAxisView VerticalAxisView { get; }

        [CanBeNull]
        public Scheduler Scheduler { get; private set; }


        private static bool IsChecked(ToggleButton checkBox)
        {
            return checkBox.IsChecked != null && checkBox.IsChecked.Value;
        }

        private bool DeveloperMode()
        {
            return TbChannel.Text == "256";
        }

        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            SwitchButton.Toggle();
        }

        private void TurnOff()
        {
            Scheduler?.Stop();
            Scheduler = null;
            GC.Collect();
        }

        private void TurnOn()
        {


            IProducer<SampleRecord> producer;
            if (DeveloperMode())
            {
                producer = new DummyProducer();
            }
            else
            {
                producer = ParallelInjector.NewProducer(IsChecked(CbCaptureSample));
            }
            Adapter = ParallelInjector.NewAdapter(CanvasView, HorizontalAxisView, VerticalAxisView);
            Writer = ParallelInjector.NewSpectrumWriter(IsChecked(CbCaptureSpec));
            var consumer = ParallelInjector.NewConsumer(producer, Adapter,Writer);
            try
            {
                Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
            }
            catch (Exception)
            {
            }
            Scheduler = new Scheduler(producer, consumer);
            consumer.FailEvent += ConsumerOnFailEvent;
            consumer.ConsumeEvent += ConsumerOnConsumeEvent;

            TbStartFreq.DataContext = Adapter;
            TbEndFreq.DataContext = Adapter;
            Scheduler.Start();
        }
        [CanBeNull]
        public SpectrumWriter Writer { get; set; }

        private void ConsumerOnConsumeEvent(object sender)
        {
            var sizeInM = SamplingConfigurations.Get().RecordLength/1e6;
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var consumedCnt = Scheduler?.Consumer.ConsumedCnt;
                var elapsedSeconds = Scheduler?.Watch.ElapsedSeconds();
                var speed = consumedCnt*sizeInM/elapsedSeconds;
                if (consumedCnt.HasValue)
                {
                    TbConsumerSpeed.Text = speed.Value.ToString("F3");
                    TbTotalData.Text = (consumedCnt.Value*sizeInM).ToString();
                }
            });
        }

        private void ConsumerOnFailEvent(object sender)
        {
            Scheduler?.Stop();
            MessageBox.Show("It seems that the source is invalid.");
            Application.Current.Dispatcher.InvokeAsync(() => { SwitchButton.Toggle(false); });
        }


        private void BnPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    TbSavePath.Text = fileName;
                }
            }
            GeneralConfigurations.Get().Directory = TbSavePath.Text;
        }


        private void OnCaptureSampleChecked(object sender, RoutedEventArgs e)
        {
            var producer = Scheduler?.Producer as SampleProducer;
            if (producer == null) return;
            if (CbCaptureSample.IsChecked != null) producer.Writer.IsOn = CbCaptureSample.IsChecked.Value;
        }

        private void CbCaptureSpec_OnChecked(object sender, RoutedEventArgs e)
        {
            if (Writer != null) Writer.IsOn = IsChecked(CbCaptureSpec);
        }

        private void DecodeFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var files = SelectFiles();
            if (!files.IsEmpty())
            {
                Task.Run(() =>
                {
                    files.ForEach(path =>
                    {
                        var deserializeData = Toolbox.DeserializeData<double[]>(path);
                        Toolbox.WriteData(path.Replace("binary", "temporal"), deserializeData);
                    });
                    MessageBox.Show("decoding finished");
                });
            }
        }

        private static string[] SelectFiles()
        {
            // Create OpenFileDialog 
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt",
                Multiselect = true
            };

            // Set filter for file extension and default file extension 


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();
            if (result == true)
            {
                return dlg.FileNames;
            }
            return new string[0];
        }

        private void LoadFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var fileNames = SelectFiles();
            if (fileNames.IsEmpty()) return;
            GeneralConfigurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
            TbSavePath.Text = GeneralConfigurations.Get().Directory;
            var producer = ParallelInjector.NewProducer(fileNames);
            Adapter = ParallelInjector.NewAdapter(CanvasView, HorizontalAxisView, VerticalAxisView);
            Writer = ParallelInjector.NewSpectrumWriter(IsChecked(CbCaptureSpec));
            var consumer = ParallelInjector.NewConsumer(producer, Adapter, Writer);
            Scheduler = new Scheduler(producer, consumer);
            Scheduler.Start();
            Scheduler = null;
        }

        private void About_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("A 2016 ST Workshop Production. All Rights Reserved.");
        }
    }
}