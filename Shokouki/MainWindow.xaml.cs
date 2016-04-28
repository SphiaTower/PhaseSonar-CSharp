using System;
using System.Windows;
using System.Windows.Media;
using Shokouki.Configs;
using Shokouki.Controllers;
using Shokouki.Model;
using Shokouki.Presenters;

namespace Shokouki
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isOn;
        public CanvasView CanvasView { get; }

        public MainWindow()
        {
            InitializeComponent();
            SamplingConfigs.Initialize(
              deviceName: "Dev2",
              channel: 0,
              samplingRate: 100e6,
              recordLength: (long)1e6,
              range: 10);

            Configurations.Initialize(
                repetitionRate: 400,
                centreSpanLength: 512,
                zeroFillFactor: 1,
                threadNum: 4,
                dispPoints: 500);
            SliceConfigs.Initialize(
                crestAmplitudeThreshold: 0.1,
                pointsBeforeCrest: 1000,
                centreSlice: true
                );

            SamplingConfigs.Get().Bind(TbDeviceName,TbChannel,TbSamplingRate,TbRecordLength,TbRange);
            Configurations.Get().Bind(TbRepRate,TbZeroFillFactor,TbCenterSpanLength,TbThreadNum,TbDispPoints);
            SliceConfigs.Get().Bind(TbPtsBeforeCrest,TbCrestMinAmp);


            CanvasView = new CanvasView(ScopeCanvas);


        }

        public Scheduler Scheduler { get; private set; }


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


        public double ScopeHeight => ScopeCanvas.ActualHeight;
        public double ScopeWidth => ScopeCanvas.ActualWidth;

        private void button_Click(object sender, RoutedEventArgs e) {
            Scheduler.Consumer.Adapter.ResetYScale();
        }

    }
}