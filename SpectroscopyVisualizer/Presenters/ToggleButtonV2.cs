using System;
using System.Windows.Controls;
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
            State = state;
            TurnOn = () => _button.Content = _onText;
            TurnOff = () => _button.Content = _offText;
        }

        public bool State {
            get { return _state; }
            set {
                _state = value;
                _button.Content = value ? _onText : _offText;
            }
        }

        [NotNull]
        public event Action TurnOn;

        [NotNull]
        public event Action TurnOff;

        public void Push() {
            if (State) {
                TurnOff();
            } else {
                TurnOn();
            }
            State = !State;
        }
    }
}