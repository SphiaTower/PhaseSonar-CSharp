using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using FTIR.Utils;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Restricted;
using Shokouki.Configs;
using Shokouki.Controllers;
using Shokouki.Factories;
using Shokouki.Presenters;
using Shokouki.Producers;

namespace Shokouki
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SamplingConfigs.Initialize(
                "Dev2",
                0,
                100,
                1,
                10);

            Configurations.Initialize(
                400,
                4,
                500,
                @"C:\buffer\captured\");
            SliceConfigs.Initialize(
                crestAmplitudeThreshold: 0.1,
                pointsBeforeCrest: 1000,
                centreSlice: true
                );
            CorrectorConfigs.Initialize(1, 512, CorrectorType.Mertz, ApodizerType.Fake);
//            CorrectorConfigs.Register(Toolbox.DeserializeData<CorrectorConfigs>(@"D:\\config.bin"));
            SamplingConfigs.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            Configurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath);
            SliceConfigs.Get().Bind(TbPtsBeforeCrest, TbCrestMinAmp);
            CorrectorConfigs.Get().Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType);
            CanvasView = new CanvasView(ScopeCanvas);
            HorizontalAxisView = new HorizontalAxisView(HorAxisCanvas);
            VerticalAxisView = new VerticalAxisView(VerAxisCanvas);
            /*     Loaded += (sender, args) =>
            {
                CbCorrector.ItemsSource = Enum.GetValues(typeof(CorrectorType)).Cast<CorrectorType>();
            };*/
            SwitchButton = new SwitchButton(ToggleButton, false, "STOP", "START", TurnOn, TurnOff);

//            Toolbox.SerializeData(@"D:\\config.bin",CorrectorConfigs.Get());
            this.SizeChanged += (sender, args) =>
            {
                Scheduler?.Consumer.Adapter.OnWindowZoomed();
            };
        }

        private SwitchButton SwitchButton { get; }


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
            if (DeveloperMode())
            {
                var dummyProducer = new DummyProducer();
                var newConsumer = Injector.NewConsumer(dummyProducer, CanvasView,HorizontalAxisView, VerticalAxisView, IsChecked(CbCaptureSpec));
                try
                {
                    newConsumer.Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                    newConsumer.Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
                }
                catch (Exception)
                {
                }
                Scheduler = new Scheduler(dummyProducer, newConsumer);
                newConsumer.FailEvent += UiConsumerOnFailEvent;
            }
            else
            {
                var sampleProducer = Injector.NewProducer(IsChecked(CbCaptureSample));
                var uiConsumer = Injector.NewConsumer(sampleProducer, CanvasView,HorizontalAxisView, VerticalAxisView, IsChecked(CbCaptureSpec));
                try
                {
                    uiConsumer.Adapter.StartFreqInMHz = Convert.ToDouble(TbStartFreq.Text); // todo move to constructor
                    uiConsumer.Adapter.EndFreqInMHz = Convert.ToDouble(TbEndFreq.Text);
                }
                catch (Exception)
                {
                }
                Scheduler = new Scheduler(sampleProducer, uiConsumer);
                uiConsumer.FailEvent += UiConsumerOnFailEvent;
            }


            TbStartFreq.DataContext = Scheduler.Consumer.Adapter;
            TbEndFreq.DataContext = Scheduler.Consumer.Adapter;

            Scheduler.Start();
        }

        private void UiConsumerOnFailEvent(object sender)
        {
            Scheduler?.Stop();
            MessageBox.Show("It seems that the source is invalid.");
            Application.Current.Dispatcher.InvokeAsync(() => { SwitchButton.Toggle(false); });
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {
            Scheduler?.Consumer.Adapter.ResetYScale();
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
            Configurations.Get().Directory = TbSavePath.Text;
        }


        private void OnCaptureSampleChecked(object sender, RoutedEventArgs e)
        {
            var producer = Scheduler?.Producer as SampleProducer;
            if (producer == null) return;
            if (CbCaptureSample.IsChecked != null) producer.Camera.IsOn = CbCaptureSample.IsChecked.Value;
        }

        private void CbCaptureSpec_OnChecked(object sender, RoutedEventArgs e)
        {
            if (Scheduler != null) Scheduler.Consumer.Save = IsChecked(CbCaptureSpec);
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
                        MessageBox.Show("decoding finished");
                    });
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
            Configurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
            TbSavePath.Text = Configurations.Get().Directory;
            var producer = Injector.NewProducer(fileNames);

            var consumer = Injector.NewConsumer(producer, CanvasView,HorizontalAxisView, VerticalAxisView, IsChecked(CbCaptureSpec));
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