using System;

namespace SpectroscopyVisualizer.Presenters {
    internal class ZoomCommand {
        private readonly DisplayAdapterV2 _adapter;
        private readonly CanvasView _canvasView;
        private readonly double _endX;
        private readonly double _startX;
        private double _lastEndFreq;

        private double _lastStartFreq;

        public ZoomCommand(double startX, double endX, CanvasView canvasView, DisplayAdapterV2 adapter) {
            _startX = startX;
            _endX = endX;
            _canvasView = canvasView;
            _adapter = adapter;
        }

        public void Invoke() {
            _lastStartFreq = _adapter.StartFreqInMHz;
            _lastEndFreq = _adapter.EndFreqInMHz;
            var width = _adapter.ScreenWidth;
            var factor = new Func<double, double>(x => x*(_lastEndFreq - _lastStartFreq) / width + _lastStartFreq);
            var nextStart = factor.Invoke(_startX);
            var nextEnd = factor.Invoke(_endX);
            _adapter.StartFreqInMHz = nextStart;
            _adapter.EndFreqInMHz = nextEnd;
            _canvasView.InvokeAsync(_canvasView.ClearLine);
        }

        public void Undo() {
            _adapter.StartFreqInMHz = _lastStartFreq;
            _adapter.EndFreqInMHz = _lastEndFreq;
        }
    }
}