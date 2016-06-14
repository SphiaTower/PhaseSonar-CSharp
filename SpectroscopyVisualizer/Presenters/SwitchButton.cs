using System;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Presenters
{
    public class SwitchButton
    {
        private readonly Action _turnOff;
        private readonly Action _turnOn;
        private bool _state;

        public SwitchButton(Button button, bool state, string onText, string offText, Action turnOn, Action turnOff)
        {
            _state = state;
            _turnOn = turnOn;
            _turnOff = turnOff;
            Button = button;
            OnText = onText;
            OffText = offText;
        }

        public Button Button { get; }
        private string OnText { get; }
        private string OffText { get; }

        public void Toggle()
        {
            if (_state)
            {
                Button.Content = OffText;
                _turnOff.Invoke();
            }
            else
            {
                Button.Content = OnText;
                _turnOn.Invoke();
            }
            _state = !_state;
        }

        public void Toggle(bool state)
        {
            _state = !state;
            Toggle();
        }
    }
}