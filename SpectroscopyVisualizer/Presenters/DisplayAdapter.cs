using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;
using NationalInstruments.Restricted;
using PhaseSonar.Correctors;

namespace SpectroscopyVisualizer.Presenters {
    public class DisplayAdapter : INotifyPropertyChanged {
        private readonly Stack<ZoomCommand> _cmdStack = new Stack<ZoomCommand>();
        private readonly HorizontalAxisView _horizontalAxisView;
        private readonly double _sampleRateInMHz;
        private readonly VerticalAxisView _verticalAxisView;
        private double _endFreqInMHz;
        private Point _lastPoint;
        private double _max;

        private double _min;
        private bool _mouseDown;
        private Func<double, double> _scaleX;
        private Func<double, double> _scaleY;
        private double _startFreqInMHz;
        private double _zoomStart;

        public DisplayAdapter(CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, int dispPointNum, double samplingRate, int startFreqInMHz = 0,
            int endFreqInMHz = 50) // todo hard coded 0 and 50
        {
            WavefromView = wavefromView;
            _horizontalAxisView = horizontalAxisView;
            _verticalAxisView = verticalAxisView;
            _sampleRateInMHz = samplingRate/1e6;
            DispPointsCnt = dispPointNum;
            StartFreqInMHz = startFreqInMHz;
            EndFreqInMHz = endFreqInMHz;
            var canvas = wavefromView.Canvas;
            canvas.MouseLeftButtonDown += (sender, args) => {
                if (!_mouseDown) {
                    _lastPoint = args.GetPosition(canvas);
                    _zoomStart = _lastPoint.X;
                    _mouseDown = true;
                }
            };
            canvas.MouseMove += (sender, args) => {
                if (!_mouseDown) return;
                var point = args.GetPosition(canvas);
                wavefromView.InvokeAsync(() => {
                    var pointCollection = new PointCollection(2) {_lastPoint, point};
                    wavefromView.DrawLine(pointCollection, Colors.Yellow);
                    _lastPoint = point;
                });
            };
            canvas.MouseLeftButtonUp += (sender, args) => {
                if (!_mouseDown) return;
                var zoomEnd = args.GetPosition(canvas).X;
                if (zoomEnd < _zoomStart) {
                    var t = zoomEnd;
                    zoomEnd = _zoomStart;
                    _zoomStart = t;
                }

                if (!(zoomEnd - _zoomStart <= 12)) {
                    var zoomCommand = new ZoomCommand(_zoomStart, zoomEnd, WavefromView, this);
                    _cmdStack.Push(zoomCommand);
                    zoomCommand.Invoke();
                    ResetYScale();
                }
                else {
                    WavefromView.ClearLine();
                }

                _mouseDown = false;
            };
            canvas.MouseRightButtonDown += (sender, args) => {
                if (_cmdStack.IsEmpty()) {
                    StartFreqInMHz = 0;
                    EndFreqInMHz = 50; // todo hard coded
                }
                else {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
                ResetYScale();
            };
            canvas.MouseDown += (sender, args) => {
                if (args.ChangedButton == MouseButton.Middle) {
                    ResetYScale();
                }
            };
            WavefromView.DrawGrid();
        }

        public CanvasView WavefromView { get; }

        public int DispPointsCnt { get; set; }

        public double ScreenHeight => WavefromView.ScopeHeight;
        public double ScreenWidth => WavefromView.ScopeWidth;

        public double EndFreqInMHz {
            get { return _endFreqInMHz; }
            set {
                _endFreqInMHz = value;
                InvokePropertyChanged("EndFreqInMHz");
                RedrawAxis();
            }
        }

        public double StartFreqInMHz {
            get { return _startFreqInMHz; }
            set {
                _startFreqInMHz = value;
                InvokePropertyChanged("StartFreqInMHz");
                RedrawAxis();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(string propertyName) {
            var e = new PropertyChangedEventArgs(propertyName);

            var changed = PropertyChanged;

            changed?.Invoke(this, e);
        }

        public PointCollection CreateGraphPoints(double[] xAxis, double[] yAxis) {
            if (_scaleX == null) _scaleX = GetXScaler(xAxis);
            if (_scaleY == null) _scaleY = GetYScaler(yAxis);
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++) {
                points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
            }
            return points;
        }

        public PointCollection CreateGraphPoints(double[] yAxis) {
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++) {
                points.Add(CreateGraphPoint(i, yAxis[i]));
            }
            return points;
        }


        public void ResetYScale() {
            _scaleY = null;
        }

        private Func<double, double> GetXScaler(double[] xAxis) {
            return x => ScreenWidth/(xAxis.Last() - xAxis.First())*x;
        }

        private static void FindMinMax([NotNull] double[] nums, out double min, out double max) {
            /*min = double.MaxValue;
            max = double.MinValue;
            foreach (var y in nums) {
                if (y > max) {
                    max = y;
                } else if (y < min) {
                    min = y;
                }
            }*/
            var length = nums.Length;
            if (length == 0) {
                throw new ArgumentOutOfRangeException();
            }
            int i;
            if (length%2 == 1) {
                max = min = nums[0];
                i = 1;
            }
            else {
                if (nums[0] > nums[1]) {
                    max = nums[0];
                    min = nums[1];
                }
                else {
                    max = nums[1];
                    min = nums[0];
                }
                i = 2;
            }
            for (; i < length; i += 2) {
                var num1 = nums[i];
                var num2 = nums[i + 1];
                if (num1 > num2) {
                    max = Math.Max(max, num1);
                    min = Math.Min(min, num2);
                }
                else {
                    max = Math.Max(max, num2);
                    min = Math.Min(min, num1);
                }
            }
        }

        private Func<double, double> GetYScaler(double[] yAxis) {
            FindMinMax(yAxis, out _min, out _max);
            var min = _min;
            var max = _max;
            _verticalAxisView.DrawRuler(min, max);
            // todo: store height as const or invoke getter to adapt
            //            const int margin = 10;
            const int margin = 0;
            var dispAreaHeight = ScreenHeight - 2*margin;
            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
            //     const int margin = 0;
            //            var dispAreaHeight = ScreenHeight - 2*margin;
            //            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
        }

        private Point CreateGraphPoint(double x, double y) {
            return new Point(_scaleX(x), _scaleY(y));
        }


        public double[] SampleAverageAndSquare(ISpectrum spec) {
            var indexOverFreq = (spec.Length() - 1)/(_sampleRateInMHz/2);
            var lo = (int) (indexOverFreq*StartFreqInMHz);
            var hi = (int) (indexOverFreq*EndFreqInMHz);

            var interval = (hi - lo)/(DispPointsCnt - 1);
            if (interval < 1) {
                while (interval < 1) {
                    var broader = (EndFreqInMHz - StartFreqInMHz)*0.05;
                    _startFreqInMHz -= broader;
                    _endFreqInMHz += broader;
                    lo = (int) (indexOverFreq*StartFreqInMHz);
                    hi = (int) (indexOverFreq*EndFreqInMHz);
                    interval = (hi - lo)/(DispPointsCnt - 1);
                }
                StartFreqInMHz = _startFreqInMHz;
                EndFreqInMHz = _endFreqInMHz;
            }

            var divider = spec.PulseCount*spec.PulseCount;
            var sampledAverPowerSpec = new double[DispPointsCnt];
            for (int i = 0, j = lo; i < DispPointsCnt; i++,j += interval) {
                sampledAverPowerSpec[i] = spec.Intensity(j)/divider;
            }
            return sampledAverPowerSpec;
        }

        public void RedrawAxis() {
            _horizontalAxisView.Canvas.Dispatcher.InvokeAsync(
                () => { _horizontalAxisView.DrawRuler(StartFreqInMHz, EndFreqInMHz); });
        }

        public void OnWindowZoomed() {
            ResetYScale();
            _horizontalAxisView.DrawRuler(StartFreqInMHz, EndFreqInMHz);
            _verticalAxisView.DrawRuler(_min, _max);
            WavefromView.DrawGrid();
        }
    }

    internal class ZoomCommand {
        private readonly DisplayAdapter _adapter;
        private readonly CanvasView _canvasView;
        private readonly double _endX;
        private readonly double _startX;
        private double _lastEndFreq;

        private double _lastStartFreq;

        public ZoomCommand(double startX, double endX, CanvasView canvasView, DisplayAdapter adapter) {
            _startX = startX;
            _endX = endX;
            _canvasView = canvasView;
            _adapter = adapter;
        }

        public void Invoke() {
            _lastStartFreq = _adapter.StartFreqInMHz;
            _lastEndFreq = _adapter.EndFreqInMHz;
            var width = _canvasView.Canvas.ActualWidth;
            var factor = new Func<double, double>(x => x/width*(_lastEndFreq - _lastStartFreq) + _lastStartFreq);
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