using System.Windows;
using System.Windows.Input;

namespace SpectroscopyVisualizer {
    /// <summary>
    ///     Interaction logic for NumberDialog.xaml
    /// </summary>
    public partial class NumberDialog : Window {
        public NumberDialog() {
            InitializeComponent();
            InputBox.DataContext = this;
            InputBox.Focus();
        }

        public int Number { get; set; } = 100;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void InputBox_OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DialogResult = true;
                Close();
            } else if (e.Key == Key.Escape) {
                DialogResult = false;
                Close();
            }
        }
    }
}