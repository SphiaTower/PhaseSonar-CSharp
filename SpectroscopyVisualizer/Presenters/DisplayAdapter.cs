using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;
using NationalInstruments.Restricted;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;

namespace SpectroscopyVisualizer.Presenters {

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
            var factor = new Func<double, double>(x => x / width * (_lastEndFreq - _lastStartFreq) + _lastStartFreq);
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
    public class DisplayAdapter : INotifyPropertyChanged {
        private readonly HorizontalAxisView _horizontalAxisView;
        private readonly double _sampleRateInMHz;
        private readonly VerticalAxisView _verticalAxisView;
        private double _endFreqInMHz;
        private double _max;

        private double _min;


        private readonly Stack<ZoomCommand> _cmdStack = new Stack<ZoomCommand>();

        private Func<double, double> _scaleX;
        private Func<double, double> _scaleY;
        private double _startFreqInMHz;
        private AxisBuilder Axis { get; }
        private readonly double[] _dummyAxis;
        private readonly double[] _instantDispValues;
        private readonly double[] _accDispValues;

//        private UIElement _prevText;

        public DisplayAdapter([NotNull] CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbXCoordinate, TextBox tbDistance, int dispPointNum, double samplingRate,  int startFreqInMHz = 0,
            int endFreqInMHz = 50) // todo hard coded 0 and 50
        {
            WavefromView = wavefromView;
            DispPointsCnt = dispPointNum;
            _instantDispValues = new double[DispPointsCnt];
            _accDispValues = new double[DispPointsCnt];
            Axis = new AxisBuilder(WavefromView);
            _dummyAxis = Axis.DummyAxis(_instantDispValues);

            _horizontalAxisView = horizontalAxisView;
            _verticalAxisView = verticalAxisView;
            _sampleRateInMHz = samplingRate/1e6;
            StartFreqInMHz = startFreqInMHz;
            EndFreqInMHz = endFreqInMHz;

            var eventLayer = EventLayer.Setup(wavefromView.Canvas);
            eventLayer.ZoomEvent += (start, end, valid) => {
                if (valid) {
                   var zoomCommand = new ZoomCommand(start, end, WavefromView, this);
                    _cmdStack.Push(zoomCommand);
                    zoomCommand.Invoke();
                    ResetYScale(); 
                } else {
                    WavefromView.ClearLine();
                }
                
            };
//            UIElement delta = null;
            eventLayer.FollowTraceEvent += (last,curr, mouseDown) => {
                double xOnAxis = GetXValueByPointPosition(curr.X);
                tbXCoordinate.Text = xOnAxis.ToString("F8");

                if (mouseDown) {
                    var xStart = GetXValueByPointPosition(eventLayer.MouseDownStart);
                    var xDelta = xOnAxis - xStart;
                    tbDistance.Text = xDelta.ToString("F8");
                }

                if (mouseDown) {
                    WavefromView.InvokeAsync(() => {
                        var pointCollection = new PointCollection(2) { last, curr };
                        WavefromView.DrawLine(pointCollection, Colors.Yellow);
                    });
                }
            };
            eventLayer.AdjustYAxisEvent += ResetYScale;
            eventLayer.UndoEvent += () => {
                if (_cmdStack.IsEmpty()) {
                    StartFreqInMHz = 0;
                    EndFreqInMHz = 50; // todo hard coded
                } else {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
                ResetYScale();
            };

            /*   var canvas = wavefromView.Canvas;
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
                } else {
                    WavefromView.ClearLine();
                }

                _mouseDown = false;
            };
            canvas.MouseRightButtonDown += (sender, args) => {
                if (_cmdStack.IsEmpty()) {
                    StartFreqInMHz = 0;
                    EndFreqInMHz = 50; // todo hard coded
                } else {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
                ResetYScale();
            };
            canvas.MouseDown += (sender, args) => {
                if (args.ChangedButton == MouseButton.Middle) {
                    ResetYScale();
                }
            };*/
            WavefromView.DrawGrid();
        }

        private double GetXValueByPointPosition(double x) {
            return x/WavefromView.ScopeWidth*(EndFreqInMHz - StartFreqInMHz) + StartFreqInMHz;
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

        [NotNull]
        public PointCollection CreateGraphPoints(double[] xAxis, [NotNull] double[] yAxis) {
            if (_scaleX == null) _scaleX = GetXScaler(xAxis);
            if (_scaleY == null) _scaleY = GetYScaler(yAxis);
      
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++) {
                points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
            }
            return points;
        }

        private ISpectrum _instantSpectrumCache;

        private ISpectrum _accumulatedSpectrumCache;

        public void UpdateData([NotNull] ISpectrum instant, [NotNull] ISpectrum accumulated) {
            // called in background
            SampleAverageAndSquare(instant,_instantDispValues);
            SampleAverageAndSquare(accumulated,_accDispValues);
            WavefromView.Canvas.Dispatcher.InvokeAsync(
                () => {
                    var instantPts = CreateGraphPoints(_dummyAxis, _instantDispValues);
                    var accPts = CreateGraphPoints(_dummyAxis, _accDispValues);
                    WavefromView.ClearWaveform();
                    WavefromView.DrawWaveform(instantPts, Colors.Red);
                    WavefromView.DrawWaveform(accPts, Colors.White);
                });
            _instantSpectrumCache = instant;
            _accumulatedSpectrumCache = accumulated;
        }
    

        public void ResetYScale() {
            _scaleX = null;
            _scaleY = null;
            UpdateData(_instantSpectrumCache,_accumulatedSpectrumCache);
        }

        [NotNull]
        private Func<double, double> GetXScaler(double[] xAxis) {
            return x => ScreenWidth/(xAxis.Last() - xAxis.First())*x;
        }

        [NotNull]
        private Func<double, double> GetYScaler([NotNull] double[] yAxis) {
            Functions.FindMinMax(yAxis, out _min, out _max);
            var min = _min;
            var max = _max;
            _verticalAxisView.Canvas.Dispatcher.InvokeAsync(() => {
                _verticalAxisView.DrawRuler(min, max);
            });
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


        public void SampleAverageAndSquare([NotNull] ISpectrum spec,double[] resultContainer) {
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
            for (int i = 0, j = lo; i < DispPointsCnt; i++,j += interval) {
                resultContainer[i] = spec.Intensity(j)/divider;
            }
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

}