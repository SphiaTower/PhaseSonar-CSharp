using System.Windows;
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
        }


        private void BnPositive_OnClick(object sender, RoutedEventArgs e) {
            SliceConfigurations.Get().CrestAtCenter = CbPeakPosition.SelectedIndex == 0;
            var config = MiscellaneousConfigurations.Get();
            config.MaxPhaseStd = double.Parse(TbMaxPhaseStd.Text);
            config.MinFlatPhasePtsNumCnt = int.Parse(TbMinPhaseLength.Text);
            config.WaitEmptyProducerMsTimeout = int.Parse(TbProducerTimeout.Text);
            Close();
        }

        private void BnNegative_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}