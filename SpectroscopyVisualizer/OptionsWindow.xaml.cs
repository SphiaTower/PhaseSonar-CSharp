using System.Windows;
using Microsoft.Win32;
using SpectroscopyVisualizer.Configs;

namespace SpectroscopyVisualizer {
    /// <summary>
    ///     Interaction logic for Options.xaml
    /// </summary>
    public partial class OptionsWindow : Window {
        public OptionsWindow() {
            InitializeComponent();
            CbPeakPosition.ItemsSource = new[] {"Center In Period", "Left In Period"};
            CbPeakPosition.SelectedIndex = SliceConfigurations.Get().CrestAtCenter ? 0 : 1;
            var config = MiscellaneousConfigurations.Get();
            TbMaxPhaseStd.Text = "" + config.MaxPhaseStd;
            TbMinPhaseLength.Text = "" + config.MinFlatPhasePtsNumCnt;
            TbProducerTimeout.Text = "" + config.WaitEmptyProducerMsTimeout;
            TbPythonPath.Text = config.PythonPath;
            TbDipLockScanRadius.Text = config.LockDipScanRadiusInMhz + "";
            CkFlipMinusSpec.IsChecked = config.AutoFlip;
        }


        private void BnPositive_OnClick(object sender, RoutedEventArgs e) {
            SliceConfigurations.Get().CrestAtCenter = CbPeakPosition.SelectedIndex == 0;
            var config = MiscellaneousConfigurations.Get();
            config.MaxPhaseStd = double.Parse(TbMaxPhaseStd.Text);
            config.MinFlatPhasePtsNumCnt = int.Parse(TbMinPhaseLength.Text);
            config.WaitEmptyProducerMsTimeout = int.Parse(TbProducerTimeout.Text);
            config.PythonPath = TbPythonPath.Text;
            config.AutoFlip = CkFlipMinusSpec.IsChecked.GetValueOrDefault(false);
            config.LockDipScanRadiusInMhz = double.Parse(TbDipLockScanRadius.Text);
            Close();
        }

        private void BnNegative_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void BnPythonPath_OnClick(object sender, RoutedEventArgs e) {
            var selectFile = SelectFile();
            if (selectFile != null) {
                TbPythonPath.Text = selectFile;
            }
        }

        public static string SelectFile() {
            // Create OpenFileDialog 
            var dlg = new OpenFileDialog {
                DefaultExt = ".exe",
                Filter = "Executable (.exe)|*.exe",
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

    }
}