using System;
using System.Windows.Controls;
using System.Windows.Media;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Presenters {
    public class ToggleButtonV2 {
        private readonly Button _button;
        private readonly string _offText;
        private readonly string _onText;
        private bool _state;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public ToggleButtonV2(Button button, bool state, string onText, string offText) {
            _button = button;
            _onText = onText;
            _offText = offText;
            TurnOn = () => {
                _button.Content = _onText;
                _button.Background = new SolidColorBrush(Colors.Yellow);
            };
            TurnOff = () => {
                _button.Content = _offText;
                _button.Background = new SolidColorBrush(Colors.FloralWhite);
            };
            State = state;
        }

        public bool State {
            get { return _state; }
            set {
                _state = value;
                if (value) {
                    TurnOn();
                } else {
                    TurnOff();
                }
            }
        }

        [NotNull]
        public event Action TurnOn;

        [NotNull]
        public event Action TurnOff;
    }
}