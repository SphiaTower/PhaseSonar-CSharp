using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Shokouki.Presenters {
    public class SwitchButton {
        public Button Button { get; }
        private bool _state;
        private readonly Action _turnOn;
        private readonly Action _turnOff;
        private string OnText { get; }
        private string OffText { get; }
        public SwitchButton(Button button, bool state, string onText, string offText, Action turnOn, Action turnOff)
        {
            _state = state;
            _turnOn = turnOn;
            _turnOff = turnOff;
            Button = button;
            OnText = onText;
            OffText = offText;
        }

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
