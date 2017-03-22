using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using JetBrains.Annotations;
using NationalInstruments.Restricted;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;

namespace SpectroscopyVisualizer.Presenters {
    public abstract class DisplayAdapterV2 : INotifyPropertyChanged {
        private readonly Stack<ZoomCommand> _cmdStack = new Stack<ZoomCommand>();
        private readonly HorizontalAxisView _horizontalAxisView;

        private double _endFreqInMHz;

        private Func<double, double> _scaleX;
        private Func<double, double> _scaleY;
        private double _startFreqInMHz;

        public DisplayAdapterV2([NotNull] CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbXCoordinate, TextBox tbDistance, int dispPointNum,
            double samplingRate, int startFreqInMHz,
            int endFreqInMHz) {
            WavefromView = wavefromView;
            WavefromView.Reload(); // todo move out, and use a event
            DispPointsCnt = dispPointNum;

            Axis = new AxisBuilder(WavefromView);

            _horizontalAxisView = horizontalAxisView;
            VerticalAxisView = verticalAxisView;
            SampleRateInMHz = samplingRate/1e6;
            StartFreqInMHz = startFreqInMHz;
            EndFreqInMHz = endFreqInMHz;

            var eventLayer = EventLayer.Setup(wavefromView.Canvas);
            eventLayer.ZoomEvent += (start, end, valid) => {
                if (valid) {
                    var zoomCommand = new ZoomCommand(start, end, WavefromView, this);
                    _cmdStack.Push(zoomCommand);
                    zoomCommand.Invoke();
                    ResetXYScales();
                } else {
                    WavefromView.ClearLine();
                }
            };
            TextBlock pop = null;
            eventLayer.FollowTraceEvent += (last, curr, mouseDown) => {
                var xOnAxis = GetXValueByPointPosition(curr.X);
                tbXCoordinate.Text = xOnAxis.ToString("F5");
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
                    tbDistance.Text = xDelta.ToString("F5");
                }

                if (mouseDown) {
                    WavefromView.InvokeAsync(() => {
                        var pointCollection = new PointCollection(2) {last, curr};
                        WavefromView.DrawLine(pointCollection, Colors.Yellow);
                    });
                }
            };
            eventLayer.AdjustYAxisEvent += ResetXYScales;
            eventLayer.UndoEvent += () => {
                if (_cmdStack.IsEmpty()) {
                    StartFreqInMHz = startFreqInMHz;
                    EndFreqInMHz = endFreqInMHz; // todo hard coded
                } else {
                    var zoomCommand = _cmdStack.Pop();
                    zoomCommand.Undo();
                }
                ResetXYScales();
            };


            WavefromView.DrawGrid();
        }

        protected double Max { get; set; }

        protected double Min { get; set; }

        protected AxisBuilder Axis { get; }

        public CanvasView WavefromView { get; }

        public int DispPointsCnt { get; set; }

        public double ScreenHeight => WavefromView.ScopeHeight;
        public double ScreenWidth => WavefromView.ScopeWidth;

        public double EndFreqInMHz {
            get { return _endFreqInMHz; }
            set {
                _endFreqInMHz = value;
                InvokePropertyChanged("EndFreqInMHz");
                OnFreqChanged();
            }
        }

        public double StartFreqInMHz {
            get { return _startFreqInMHz; }
            set {
                _startFreqInMHz = value;
                InvokePropertyChanged("StartFreqInMHz");
//                OnFreqChanged();
            }
        }

        public double SampleRateInMHz { get; set; }

        public VerticalAxisView VerticalAxisView { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetStartFreq(double freqInMHz) {
            _startFreqInMHz = freqInMHz;
        }

        protected void SetEndFreq(double freqInMHz) {
            _endFreqInMHz = freqInMHz;
        }

        private double GetXValueByPointPosition(double x) {
            return x*(EndFreqInMHz - StartFreqInMHz) / ScreenWidth + StartFreqInMHz;
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

            if (points.IsEmpty())
                for (var i = 0; i < yAxis.Length; i++) {
                    points.Add(CreateGraphPoint(xAxis[i], yAxis[i]));
                }
            else
                for (var i = 0; i < yAxis.Length; i++) {
                    //                    // todo data overflow
                    //                    if (yAxis[i].IsSpecialValue()) {
                    //                        yAxis[i] = 0;
                    //                    }
                    points[i] = CreateGraphPoint(xAxis[i], yAxis[i]);
                }
            return points;
        }


        public void ResetXYScales() {
            _scaleX = null;
            _scaleY = null;
            InflateCache();
        }

        public abstract void InflateCache();

        public void ResetXScales() {
            _scaleY = null;
        }

        public void ResetYScales() {
            _scaleY = null;
        }

        public static int GetIndexFromFreq(double freq, double sampleRateInMHz, int specLength) {
            var freqInterval = sampleRateInMHz / (specLength * 2 - 1);
            var startFreqBeyondZero = freqInterval / 2;
            return (int)Math.Round((freq - startFreqBeyondZero) / freqInterval);
        }

        [NotNull]
        private Func<double, double> GetXScaler(double[] xAxis) {
            return x => ScreenWidth/(xAxis.Last() - xAxis.First())*x;
        }

        protected abstract Func<double, double> GetYScaler([NotNull] double[] yAxis);


        private Point CreateGraphPoint(double x, double y) {
            return new Point(_scaleX(x), _scaleY(y));
        }


        public void OnFreqChanged() {
            _horizontalAxisView.Canvas.Dispatcher.InvokeAsync(
                () => { _horizontalAxisView.DrawRuler(StartFreqInMHz, EndFreqInMHz); });
        }

        public virtual void OnWindowZoomed() {
            ResetXYScales();
            _horizontalAxisView.DrawRuler(StartFreqInMHz, EndFreqInMHz);
            VerticalAxisView.DrawRuler(Min, Max,this is PhaseDisplayAdapter);
            WavefromView.Reload();
            WavefromView.DrawGrid();
        }
    }

    public class PhaseDisplayAdapter : DisplayAdapterV2 {
        private readonly double[] _dummyAxis;

        private readonly double[] _instantDispValues;

        private readonly PointCollection _instantPts;

        [CanBeNull] private double[] _instantPhaseCache;

        public PhaseDisplayAdapter([NotNull] CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbXCoordinate, TextBox tbDistance, int dispPointNum,
            double samplingRate, int startFreqInMHz, int endFreqInMHz)
            : base(
                wavefromView, horizontalAxisView, verticalAxisView, tbXCoordinate, tbDistance, dispPointNum,
                samplingRate, startFreqInMHz, endFreqInMHz) {
            _instantDispValues = new double[DispPointsCnt];

            _instantPts = new PointCollection(DispPointsCnt);
            _dummyAxis = Axis.DummyAxis(_instantDispValues);
        }

        public void UpdatePhase([NotNull] double[] instant) {
            ResetYScales();
            SampleAverage(instant, _instantDispValues);
            WavefromView.Canvas.Dispatcher.InvokeAsync(
                () => {
                    var instantPts = CreateGraphPoints(_dummyAxis, _instantDispValues, _instantPts);
                    WavefromView.DrawWaveform(instantPts, Colors.OrangeRed, 0);
                });
            _instantPhaseCache = instant;
        }


        public override void InflateCache() {
            if (_instantPhaseCache == null) {
                return;
            }
            UpdatePhase(_instantPhaseCache);
        }
      
        [NotNull]
        protected override Func<double, double> GetYScaler([NotNull] double[] yAxis) {
            double min, max;
            Functions.FindMinMax(yAxis, out min, out max);
            Min = min;
            Max = max;
            VerticalAxisView.Canvas.Dispatcher.InvokeAsync(() => { VerticalAxisView.DrawRuler(min, max,true); });
            // todo: store height as const or invoke getter to adapt
            //            const int margin = 10;
            const int margin = 0;
            var dispAreaHeight = ScreenHeight - 2*margin;
            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
            //     const int margin = 0;
            //            var dispAreaHeight = ScreenHeight - 2*margin;
            //            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
        }

        public void SampleAverage([NotNull] double[] phase, double[] resultContainer) {
//            var indexOverFreq = (phase.Length - 1)/(SampleRateInMHz/2);
            var lo = GetIndexFromFreq(StartFreqInMHz, SampleRateInMHz, phase.Length);
            var hi = GetIndexFromFreq(EndFreqInMHz, SampleRateInMHz, phase.Length);

            double interval = (hi - lo)/(double)(DispPointsCnt - 1);
         /*   if (interval < 1) {
                while (interval < 1) {
                    var broader = (EndFreqInMHz - StartFreqInMHz)*0.05;
                    SetStartFreq(StartFreqInMHz - broader);
                    SetEndFreq(EndFreqInMHz + broader);
                    lo = (int) (indexOverFreq*StartFreqInMHz);
                    hi = (int) (indexOverFreq*EndFreqInMHz);
                    interval = (hi - lo)/ (double)(DispPointsCnt - 1);
                }
                StartFreqInMHz = StartFreqInMHz;
                EndFreqInMHz = EndFreqInMHz;
            }*/
            double j = lo;
            for (int i = 0; i < DispPointsCnt; i++, j += interval) {
                resultContainer[i] = phase[(int)Math.Round(j)];
            }
        }
    }

    public class SpectrumDisplayAdapter : DisplayAdapterV2 {
        private readonly double[] _accDispValues;
        private readonly PointCollection _accPts;

        private readonly double[] _dummyAxis;

        private readonly double[] _instantDispValues;
        private readonly PointCollection _instantPts;
        private readonly double _lockDipScanRadius;

        [CanBeNull] private ISpectrum _accumulatedSpectrumCache;

        private readonly Ellipse _ellipse = new Ellipse {
            Width = 40,
            Height = 40,
            Stroke = new SolidColorBrush(Color.FromArgb(255, 220, 200, 0)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection {14, 4},
            Visibility = Visibility.Hidden
        };

        private readonly Ellipse _innerEllipse = new Ellipse {
            Width = 30,
            Height = 30,
            Stroke = new SolidColorBrush(Color.FromArgb(255, 220, 200, 0)),
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection {11, 3},
            Visibility = Visibility.Hidden

        };

        [CanBeNull] private ISpectrum _instantSpectrumCache;

        private double? _lockDipFreq;

        public SpectrumDisplayAdapter([NotNull] CanvasView wavefromView, HorizontalAxisView horizontalAxisView,
            VerticalAxisView verticalAxisView, TextBox tbXCoordinate, TextBox tbDistance, int dispPointNum,
            double samplingRate, int startFreqInMHz, int endFreqInMHz, double? lockDipFreq, double lockDipScanRadius)
            : base(
                wavefromView, horizontalAxisView, verticalAxisView, tbXCoordinate, tbDistance, dispPointNum,
                samplingRate, startFreqInMHz, endFreqInMHz) {
            _lockDipFreq = lockDipFreq;
            _lockDipScanRadius = lockDipScanRadius;
            _instantDispValues = new double[DispPointsCnt];
            _accDispValues = new double[DispPointsCnt];

            _instantPts = new PointCollection(DispPointsCnt);
            _accPts = new PointCollection(DispPointsCnt);
            _dummyAxis = Axis.DummyAxis(_instantDispValues);
            Min = 0;
            if (lockDipFreq.HasValue) {

                WavefromView.Canvas.Children.Add(_ellipse);
                WavefromView.Canvas.Children.Add(_innerEllipse);
            }
        }

        [NotNull]
        protected override Func<double, double> GetYScaler([NotNull] double[] yAxis) {
            Max = yAxis.Max();
            VerticalAxisView.Canvas.Dispatcher.InvokeAsync(() => { VerticalAxisView.DrawRuler(Min, Max,false); });
            // todo: store height as const or invoke getter to adapt
            //            const int margin = 10;
            const int margin = 0;
            var dispAreaHeight = ScreenHeight - 2*margin;
            return y => dispAreaHeight - dispAreaHeight/Max*y + margin;
            //     const int margin = 0;
            //            var dispAreaHeight = ScreenHeight - 2*margin;
            //            return y => dispAreaHeight - dispAreaHeight/(max - min)*(y - min) + margin;
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
                    DrawLockIndicator();
                    WavefromView.DrawWaveform(instantPts, Colors.OrangeRed, 0);
                    WavefromView.DrawWaveform(accPts, Colors.White, 1);
                });
            _instantSpectrumCache = instant;
            _accumulatedSpectrumCache = accumulated;
        }

        private double _lockX;
        private double _lockY;

        private void DrawLockIndicator() {
            if (_lockDipFreq.HasValue) {
                if (_lockDipFreq < EndFreqInMHz && _lockDipFreq > StartFreqInMHz) {
                    var portion = (_lockDipFreq.Value - StartFreqInMHz)/(EndFreqInMHz - StartFreqInMHz);
                    var xIndex = (int) (portion*DispPointsCnt);

                    var span = (int) (_lockDipScanRadius/(EndFreqInMHz - StartFreqInMHz)*DispPointsCnt);
                    var dipIndex = Functions.FindMinIndex(_accDispValues, Math.Max(xIndex - span, 0),
                        Math.Min(xIndex + span, DispPointsCnt));
                    var x = _accPts[dipIndex].X;

                    var y = _accPts[dipIndex].Y;

                    Canvas.SetTop(_ellipse, y - 20);
                    Canvas.SetLeft(_ellipse, x - 20);
                    Canvas.SetTop(_innerEllipse, y - 15);
                    Canvas.SetLeft(_innerEllipse, x - 15);
                    _ellipse.StrokeDashOffset -= 1.7;
                    _innerEllipse.StrokeDashOffset += 0.7;
                    if (_ellipse.Visibility!=Visibility.Visible) {
                        _ellipse.Visibility = Visibility.Visible;
                        _innerEllipse.Visibility = Visibility.Visible; ;
                    }
                } else {
                    _ellipse.Visibility = Visibility.Hidden;
                    _innerEllipse.Visibility = Visibility.Hidden;;
                }
            }
        }

      

        public void SampleAverageAndSquare([NotNull] ISpectrum spec, double[] resultContainer) {
            var lo = GetIndexFromFreq(StartFreqInMHz,SampleRateInMHz,spec.Length());
            var hi = GetIndexFromFreq(EndFreqInMHz,SampleRateInMHz,spec.Length());

            

            var interval = (hi - lo)/ (double)(DispPointsCnt - 1);
            /*if (interval < 1) {
                while (interval < 1) {
                    var broader = (EndFreqInMHz - StartFreqInMHz)*0.05;
                    SetStartFreq(StartFreqInMHz - broader);
                    SetEndFreq(EndFreqInMHz + broader);
                    lo = GetIndexFromFreq(StartFreqInMHz, SampleRateInMHz, spec.Length());
                    hi = GetIndexFromFreq(EndFreqInMHz, SampleRateInMHz, spec.Length());
                    interval = (hi - lo)/ (double)(DispPointsCnt - 1);
                }
                StartFreqInMHz = StartFreqInMHz;
                EndFreqInMHz = EndFreqInMHz;
            }*/

            var divider = spec.PulseCount*spec.PulseCount;
            double j = lo;
            for (int i = 0; i < DispPointsCnt; i++, j += interval) {
                resultContainer[i] = spec.Intensity((int)Math.Round(j))/divider;
            }
        }

        public override void InflateCache() {
            if (_instantSpectrumCache == null) {
                return;
            }
            UpdateData(_instantSpectrumCache, _accumulatedSpectrumCache);
        }

        public override void OnWindowZoomed() {
            base.OnWindowZoomed();
            if (_lockDipFreq.HasValue) {

                WavefromView.Canvas.Children.Add(_ellipse);
                WavefromView.Canvas.Children.Add(_innerEllipse);
            }
        }
    }
}