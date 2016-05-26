using System;
using System.IO;
using System.Windows;
using JetBrains.Annotations;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        private bool _isOn;

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
            SamplingConfigs.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            Configurations.Get().Bind(TbRepRate, TbThreadNum, TbDispPoints, TbSavePath);
            SliceConfigs.Get().Bind(TbPtsBeforeCrest, TbCrestMinAmp);
            CorrectorConfigs.Get().Bind(TbZeroFillFactor, TbCenterSpanLength, CbCorrector, CbApodizationType);
            CanvasView = new CanvasView(ScopeCanvas);
            /*     Loaded += (sender, args) =>
            {
                CbCorrector.ItemsSource = Enum.GetValues(typeof(CorrectorType)).Cast<CorrectorType>();
            };*/
        }


        public CanvasView CanvasView { get; }

        [CanBeNull]
        public Scheduler Scheduler { get; private set; }

        private bool SaveSample => CbCaptureSample.IsChecked != null && CbCaptureSample.IsChecked.Value;
        private bool SaveSpec => CbCaptureSpec.IsChecked != null && CbCaptureSpec.IsChecked.Value;


        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_isOn)
            {
                ToggleButton.Content = "START";
                Scheduler.Stop();
                Scheduler = null;
                //MessageBox.Show("refreshCnt" + _refreshCnt);
            }
            else
            {
                var sampleProducer = Injector.NewProducer(SaveSample);
                if (CbCaptureSample.IsChecked != null) sampleProducer.Camera.IsOn = CbCaptureSample.IsChecked.Value;
                var uiConsumer = Injector.NewConsumer(sampleProducer, CanvasView, SaveSpec);
                try
                {
                    uiConsumer.Adapter.StartFreqInMHz = Convert.ToDouble(StartFreqTextBox.Text); // todo move to constructor
                    uiConsumer.Adapter.EndFreqInMHz = Convert.ToDouble(EndFreqTextBox.Text);
                } catch (Exception)
                {
                }
                Scheduler = new Scheduler(sampleProducer, uiConsumer);

                StartFreqTextBox.DataContext = Scheduler.Consumer.Adapter;
                EndFreqTextBox.DataContext = Scheduler.Consumer.Adapter;

                ToggleButton.Content = "STOP";
                Scheduler.Start();
            }
            _isOn = !_isOn;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Scheduler?.Consumer.Adapter.ResetYScale();
        }

        private void BnLoad_Click(object sender, RoutedEventArgs e)
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


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                var fileNames = dlg.FileNames;
                Configurations.Get().Directory = Path.GetDirectoryName(fileNames[0]) + @"\";
                TbSavePath.Text = Configurations.Get().Directory;
                var producer = Injector.NewProducer(fileNames);
//                var spectrumCamera = Injector.NewCamera<RealSpectrum>();
                /* Scheduler = new Scheduler(producer,
                    new SimpleSpetrumViewer<RealSpectrum>(producer.BlockingQueue, CanvasView,
                        Injector.NewAccumulator<RealSpectrum>(),
                        Injector.NewAdapter(CanvasView),
                        spectrumCamera));*/
                var consumer = Injector.NewConsumer(producer, CanvasView, SaveSpec);
                Scheduler = new Scheduler(producer, consumer);
                Scheduler.Start();
                BnLoad.Content = "Loading";
                producer.OnDataLoadedListener = () => BnLoad.Content = "Load";
                Scheduler = null;
            }
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
            if (Scheduler != null) Scheduler.Consumer.Save = SaveSpec;
        }
    }
}