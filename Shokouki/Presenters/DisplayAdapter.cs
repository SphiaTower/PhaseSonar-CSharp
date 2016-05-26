using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FTIR.Correctors;
using NationalInstruments.Restricted;

namespace Shokouki.Presenters
{
    public class DisplayAdapter : INotifyPropertyChanged
    {
        private readonly Stack<ZoomCommand> _cmdStack = new Stack<ZoomCommand>();
        private readonly double _sampleRateInMHz;
        private readonly CanvasView _view;
        private double _endFreqInMHz;
        private Point _lastPoint;
        private bool _mouseDown;
        private Func<double, double> _scaleX;
        private Func<double, double> _scaleY;
        private double _startFreqInMHz;
        private double _zoomStart;

        public DisplayAdapter(CanvasView view, int dispPointNum, double samplingRate, int startFreqInMHz = 0,
            int endFreqInMHz = 50)
        {
            _view = view;
            _sampleRateInMHz = samplingRate/1e6;
            DispPointsCnt = dispPointNum;
            StartFreqInMHz = startFreqInMHz;
            EndFreqInMHz = endFreqInMHz;
            var canvas = view.Canvas;
            canvas.MouseLeftButtonDown += (sender, args) =>
            {
                if (!_mouseDown)
                {
                    _lastPoint = args.GetPosition(canvas);
                    _zoomStart = _lastPoint.X;
                    _mouseDown = true;
                }
            };
            canvas.MouseMove += (sender, args) =>
            {
                if (!_mouseDown) return;
                var point = args.GetPosition(canvas);
                view.InvokeAsync(() =>
                {
                    var pointCollection = new PointCollection(2) {_lastPoint, point};
                    view.DrawLine(pointCollection, Colors.Yellow);
                    _lastPoint = point;
                });
            };
            canvas.MouseLeftButtonUp += (sender, args) =>
            {
                if (!_mouseDown) return;
                var zoomEnd = args.GetPosition(canvas).X;
                if (zoomEnd < _zoomStart)
                {
                    var t = zoomEnd;
                    zoomEnd = _zoomStart;
                    _zoomStart = t;
                }

                if (!(zoomEnd - _zoomStart <= 12))
                {
                    var zoomCommand = new ZoomCommand(_zoomStart, zoomEnd, _view, this);
                    _cmdStack.Push(zoomCommand);
                    zoomCommand.Invoke();
                }
                else
                {
                    _view.ClearLine();
                }

                _mouseDown = false;
            };
            canvas.MouseRightButtonDown += (sender, args) =>
            {
                if (_cmdStack.IsEmpty())
                {
                    StartFreqInMHz = 0;
                    EndFreqInMHz = 50; // todo hard coded
                }
                else
                {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
            };
        }

        public int DispPointsCnt { get; set; }

        public double ScreenHeight => _view.ScopeHeight;
        public double ScreenWidth => _view.ScopeWidth;

        public double EndFreqInMHz
        {
            get { return _endFreqInMHz; }
            set
            {
                _endFreqInMHz = value;
                InvokePropertyChanged("EndFreqInMHz");
            }
        }

        public double StartFreqInMHz
        {
            get { return _startFreqInMHz; }
            set
            {
                _startFreqInMHz = value;
                InvokePropertyChanged("StartFreqInMHz");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InvokePropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);

            var changed = PropertyChanged;

            if (changed != null) changed(this, e);
        }

        public PointCollection CreateGraphPoints(double[] xAxis, double[] yAxis)
        {
            if (_scaleX == null) _scaleX = GetXScaler(xAxis);
            if (_scaleY == null) _scaleY = GetYScaler(yAxis);
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++)
            {
                points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
            }
            return points;
        }

        public PointCollection CreateGraphPoints(double[] yAxis)
        {
            /*  if (_xScale < 0)
            {
                _xScale = ScreenWidth/(yAxis.Length-0);
            }
            if (_yScale < 0) {
                _yScale = ComputeYScale(yAxis);
            }*/ // todo
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++)
            {
                points.Add(CreateGraphPoint(i, yAxis[i]));
            }
            return points;
        }


        public void ResetYScale()
        {
            _scaleY = null;
        }

        private Func<double, double> GetXScaler(double[] xAxis)
        {
            return x => ScreenWidth/(xAxis.Last() - xAxis.First())*x;
        }


        private Func<double, double> GetYScaler(double[] yAxis)
        {
            double min = double.MaxValue, max = double.MinValue;
            foreach (var y in yAxis)
            {
                if (y > max)
                {
                    max = y;
                }
                else if (y < min)
                {
                    min = y;
                }
            }
            // todo: store height as const or invoke getter to adapt
            const int margin = 10;
            var dispAreaHeight = ScreenHeight - 2*margin;
            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
        }

        private Point CreateGraphPoint(double x, double y)
        {
            return new Point(_scaleX(x), _scaleY(y));
        }


        public double[] SampleAverageAndSquare(ISpectrum spec)
        {
            var indexOverFreq = (spec.Length() - 1)/(_sampleRateInMHz/2);
            var lo = (int) (indexOverFreq*StartFreqInMHz);
            var hi = (int) (indexOverFreq*EndFreqInMHz);

            var interval = (hi - lo)/(DispPointsCnt - 1);
            while (interval < 1)
            {
                var broader = (EndFreqInMHz - StartFreqInMHz)*0.05;
                StartFreqInMHz -= broader;
                EndFreqInMHz += broader;
                lo = (int) (indexOverFreq*StartFreqInMHz);
                hi = (int) (indexOverFreq*EndFreqInMHz);
                interval = (hi - lo)/(DispPointsCnt - 1);
            }
            var divider = spec.PulseCount*spec.PulseCount;
            var sampledAverPowerSpec = new double[DispPointsCnt];
            for (int i = 0, j = lo; i < DispPointsCnt; i++,j += interval)
            {
                sampledAverPowerSpec[i] = spec.Power(j)/divider;
            }
            return sampledAverPowerSpec;
        }
    }

    internal class ZoomCommand
    {
        private readonly DisplayAdapter _adapter;
        private readonly CanvasView _canvasView;
        private readonly double _endX;
        private readonly double _startX;
        private double _lastEndFreq;

        private double _lastStartFreq;

        public ZoomCommand(double startX, double endX, CanvasView canvasView, DisplayAdapter adapter)
        {
            _startX = startX;
            _endX = endX;
            _canvasView = canvasView;
            _adapter = adapter;
        }

        public void Invoke()
        {
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

        public void Undo()
        {
            _adapter.StartFreqInMHz = _lastStartFreq;
            _adapter.EndFreqInMHz = _lastEndFreq;
        }
    }
}