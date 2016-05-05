using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NationalInstruments.Restricted;
using Shokouki.Configs;
using Shokouki.Consumers;
using Shokouki.Controllers;
using Shokouki.Presenters;

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
                100e6,
                (long) 1e6,
                10);

            Configurations.Initialize(
                400,
                512,
                1,
                4,
                500);
            SliceConfigs.Initialize(
                crestAmplitudeThreshold: 0.1,
                pointsBeforeCrest: 1000,
                centreSlice: true
                );

            SamplingConfigs.Get().Bind(TbDeviceName, TbChannel, TbSamplingRate, TbRecordLength, TbRange);
            Configurations.Get().Bind(TbRepRate, TbZeroFillFactor, TbCenterSpanLength, TbThreadNum, TbDispPoints);
            SliceConfigs.Get().Bind(TbPtsBeforeCrest, TbCrestMinAmp);


            CanvasView = new CanvasView(ScopeCanvas);
        }

        public CanvasView CanvasView { get; }

        public Scheduler Scheduler { get; private set; }


        public double ScopeHeight => ScopeCanvas.ActualHeight;
        public double ScopeWidth => ScopeCanvas.ActualWidth;


        private void ToggleButton_OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_isOn)
            {
                ToggleButton.Content = "START";
                Scheduler.Stop();
                //MessageBox.Show("refreshCnt" + _refreshCnt);
            }
            else
            {
                Scheduler = new Scheduler(CanvasView);

                StartFreqTextBox.DataContext = Scheduler.Consumer.Adapter;
                EndFreqTextBox.DataContext = Scheduler.Consumer.Adapter;

                ToggleButton.Content = "STOP";
                Scheduler.Start();
            }
            _isOn = !_isOn;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Scheduler.Consumer.Adapter.ResetYScale();
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
                var producer = Injector.NewProducer(fileNames);
                Scheduler = new Scheduler(producer,
                    new SimpleSpetrumViewer(producer.BlockingQueue, CanvasView, Injector.NewAccumulator(),
                        Injector.NewAdapter(CanvasView)));
                Scheduler.Start();
                BnLoad.Content = "Loading";
                producer.OnDataLoadedListener = () => { BnLoad.Dispatcher.Invoke(() => BnLoad.Content = "Load"); };
            }
        }

        private void BnPath_Click(object sender, RoutedEventArgs e) {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    TbSavePath.Text = fileName;
                }
            }
        }
    }
}