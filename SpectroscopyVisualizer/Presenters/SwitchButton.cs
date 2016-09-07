using System;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Presenters {
    /// <summary>
    ///     An button which encapsulates toggle operations.
    /// </summary>
    public class SwitchButton {
        private readonly Action _turnOff;
        private readonly Action _turnOn;
        private bool _state;

        /// <summary>
        ///     Create an instance.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
        /// <param name="onText"></param>
        /// <param name="offText"></param>
        /// <param name="turnOn"></param>
        /// <param name="turnOff"></param>
        public SwitchButton(Button button, bool state, string onText, string offText, Action turnOn, Action turnOff) {
            _state = state;
            _turnOn = turnOn;
            _turnOff = turnOff;
            Button = button;
            OnText = onText;
            OffText = offText;
        }

        /// <summary>
        ///     The concrete button object.
        /// </summary>
        public Button Button { get; }

        private string OnText { get; }
        private string OffText { get; }

        /// <summary>
        ///     Toggle the button.
        /// </summary>
        public void Toggle() {
            if (_state) {
                Button.Content = OffText;
                _turnOff.Invoke();
            } else {
                Button.Content = OnText;
                _turnOn.Invoke();
            }
            _state = !_state;
        }

        /// <summary>
        ///     Switch to a specified state.
        /// </summary>
        /// <param name="state"></param>
        public void Toggle(bool state) {
            _state = !state;
            Toggle();
        }
    }
}