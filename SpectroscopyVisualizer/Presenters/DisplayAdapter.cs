using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

    public class DisplayAdapter : INotifyPropertyChanged {
        private readonly double[] _accDispValues;
        private readonly PointCollection _accPts;


        private readonly Stack<ZoomCommand> _cmdStack = new Stack<ZoomCommand>();
        private readonly double[] _dummyAxis;
        private readonly HorizontalAxisView _horizontalAxisView;
        private readonly double[] _instantDispValues;

        private readonly PointCollection _instantPts;
        private readonly double _sampleRateInMHz;
        private readonly VerticalAxisView _verticalAxisView;
        [CanBeNull]
        private ISpectrum _accumulatedSpectrumCache;
        private double _endFreqInMHz;
        [CanBeNull]
        private ISpectrum _instantSpectrumCache;
        private double _max;

        private double _min;

        private Func<double, double> _scaleX;
        private Func<double, double> _scaleY;
        private double _startFreqInMHz;

//        private UIElement _prevText;

        public DisplayAdapter([NotNull] CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbXCoordinate, TextBox tbDistance, int dispPointNum,
            double samplingRate, int startFreqInMHz,
            int endFreqInMHz) // todo hard coded 0 and 50
        {
            WavefromView = wavefromView;
            WavefromView.Reload(); // todo move out, and use a event
            DispPointsCnt = dispPointNum;
            _instantDispValues = new double[DispPointsCnt];
            _accDispValues = new double[DispPointsCnt];

            _instantPts = new PointCollection(DispPointsCnt);
            _accPts = new PointCollection(DispPointsCnt);
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
            TextBlock pop = null;
            eventLayer.FollowTraceEvent += (last, curr, mouseDown) => {
                var xOnAxis = GetXValueByPointPosition(curr.X);
                tbXCoordinate.Text = xOnAxis.ToString("F8");
                if (pop == null) {
                    pop = WavefromView.DrawText(curr.X + 4, curr.Y - 12, xOnAxis.ToString("F3"));
                } else {
                    Canvas.SetTop(pop, curr.Y - 12);
                    Canvas.SetLeft(pop, curr.X + 4);
                    pop.Text = xOnAxis.ToString("F3");
                }
                if (mouseDown) {
                    var xStart = GetXValueByPointPosition(eventLayer.MouseDownStart);
                    var xDelta = xOnAxis - xStart;
                    tbDistance.Text = xDelta.ToString("F8");
                }

                if (mouseDown) {
                    WavefromView.InvokeAsync(() => {
                        var pointCollection = new PointCollection(2) {last, curr};
                        WavefromView.DrawLine(pointCollection, Colors.Yellow);
                    });
                }
            };
            eventLayer.AdjustYAxisEvent += ResetYScale;
            eventLayer.UndoEvent += () => {
                if (_cmdStack.IsEmpty()) {
                    StartFreqInMHz = startFreqInMHz;
                    EndFreqInMHz = endFreqInMHz; // todo hard coded
                } else {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
                ResetYScale();
            };


            WavefromView.DrawGrid();
        }

        private AxisBuilder Axis { get; }

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

        private double GetXValueByPointPosition(double x) {
            return x/WavefromView.ScopeWidth*(EndFreqInMHz - StartFreqInMHz) + StartFreqInMHz;
        }

        private void InvokePropertyChanged(string propertyName) {
            var e = new PropertyChangedEventArgs(propertyName);

            var changed = PropertyChanged;

            changed?.Invoke(this, e);
        }

        [NotNull]
        public PointCollection CreateGraphPoints(double[] xAxis, [NotNull] double[] yAxis, PointCollection points) {
            if (_scaleX == null) _scaleX = GetXScaler(xAxis);
            if (_scaleY == null) _scaleY = GetYScaler(yAxis);

//            var points = new PointCollection(yAxis.Length);
            if (points.IsEmpty())
                for (var i = 0; i < yAxis.Length; i++) {
                    points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
                }
            else
                for (var i = 0; i < yAxis.Length; i++) {
                    //                points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
                    points[i] = CreateGraphPoint(xAxis[i], yAxis[i]);
                }
            return points;
        }

        public void UpdateData([NotNull] ISpectrum instant, [NotNull] ISpectrum accumulated) {
            // called in background
            SampleAverageAndSquare(instant, _instantDispValues);
            SampleAverageAndSquare(accumulated, _accDispValues);
            WavefromView.Canvas.Dispatcher.InvokeAsync(
                () => {
                    var instantPts = CreateGraphPoints(_dummyAxis, _instantDispValues, _instantPts);
                    var accPts = CreateGraphPoints(_dummyAxis, _accDispValues, _accPts);
//                    WavefromView.ClearWaveform();
                    WavefromView.DrawWaveform(instantPts, Colors.OrangeRed, 0);
                    WavefromView.DrawWaveform(accPts, Colors.White, 1);
                });
            _instantSpectrumCache = instant;
            _accumulatedSpectrumCache = accumulated;
        }

        public void UpdateData([NotNull] ISpectrum instant) {
            // todo bug!
            // called in background
            SampleAverage(instant, _instantDispValues);
            WavefromView.Canvas.Dispatcher.InvokeAsync(
                () => {
                    var instantPts = CreateGraphPoints(_dummyAxis, _instantDispValues, _instantPts);
//                    WavefromView.ClearWaveform();
                    WavefromView.DrawWaveform(instantPts, Colors.OrangeRed, 0);
                });
            _instantSpectrumCache = instant;
        }


        public void ResetYScale() {
            _scaleX = null;
            _scaleY = null;
            if (_instantSpectrumCache==null) {
                return;
            }
            if (_accumulatedSpectrumCache==null) {
                UpdateData(_instantSpectrumCache);
            } else {
                UpdateData(_instantSpectrumCache, _accumulatedSpectrumCache);
            }
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
            _verticalAxisView.Canvas.Dispatcher.InvokeAsync(() => { _verticalAxisView.DrawRuler(min, max); });
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


        public void SampleAverageAndSquare([NotNull] ISpectrum spec, double[] resultContainer) {
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

        public void SampleAverage([NotNull] ISpectrum spec, double[] resultContainer) {
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

            var divider = spec.PulseCount;
            for (int i = 0, j = lo; i < DispPointsCnt; i++,j += interval) {
                resultContainer[i] = spec.Magnitude(j)/divider;
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
            WavefromView.Reload();
            WavefromView.DrawGrid();
        }
    }
}