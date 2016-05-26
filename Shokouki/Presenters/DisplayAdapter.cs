using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FTIR.Correctors;

namespace Shokouki.Presenters
{
    public class DisplayAdapter
    {
        private readonly double _samplingRateInMHz;
        private readonly IScopeView _view;

        private Func<double, double> _scaleX;

        private Func<double, double> _scaleY;

        public DisplayAdapter(IScopeView view, int dispPointNum, double samplingRate,int startFreqInMHz=0,int endFreqInMHz=50)
        {
            _view = view;
            _samplingRateInMHz = samplingRate/1e6;
            DispPointsCnt = dispPointNum; //todo bind
            StartFreqInMHz = startFreqInMHz;
            EndFreqInMHz = endFreqInMHz;
            var v = view as CanvasView;
            Canvas canvas = v.Canvas;
            canvas.MouseLeftButtonDown += (sender, args) =>
            {
                var x = args.GetPosition(canvas).X;
                if (!_mouseDown)
                {
                    _zoom[0] = x;
                    _mouseDown = true;
                }
                else
                {
                    _zoom[1] = x;
                    _mouseDown = false;
                    var nextStart = _zoom[0]/canvas.ActualWidth*(EndFreqInMHz - StartFreqInMHz) + StartFreqInMHz;
                    var nextEnd =  _zoom[1]/canvas.ActualWidth * (EndFreqInMHz - StartFreqInMHz) + StartFreqInMHz;
                    StartFreqInMHz = nextStart;
                    EndFreqInMHz = nextEnd;
                }
            };
        }

        private double[] _zoom = new double[2];
        private bool _mouseDown = false;
        public int DispPointsCnt { get; set; }

        public double ScreenHeight => _view.ScopeHeight;
        public double ScreenWidth => _view.ScopeWidth;
        public double EndFreqInMHz { get; set; }
        public double StartFreqInMHz { get; set; }


        public PointCollection ToPoints(double[] xAxis, double[] yAxis)
        {
            if (_scaleX == null)
            {
                _scaleX = GetXScaler(xAxis);
            }
            if (_scaleY == null)
            {
                _scaleY = GetYScaler(yAxis);
            }
            var points = new PointCollection(yAxis.Length);
            for (var i = 0; i < yAxis.Length; i++)
            {
                points.Add(Scale(xAxis[i], yAxis[i]));
            }
            return points;
        }

        public PointCollection ToPoints(double[] yAxis)
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
                points.Add(Scale(i, yAxis[i]));
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
            return y => ScreenHeight - ScreenHeight/(max - min)*(y - min);
        }

        private Point Scale(double x, double y)
        {
            return new Point(_scaleX(x), _scaleY(y));
            // todo translate
        }


        public double[] DownSample(double[] spetrum)
        {
            return DownSampleAndAverage(spetrum, 1);
        }

        public double[] DownSampleAndAverage(double[] spetrum, int periodCnt)
        {
            var lo = (int) (spetrum.Length*StartFreqInMHz/(_samplingRateInMHz/2));
            var hi = (int) (spetrum.Length*EndFreqInMHz/(_samplingRateInMHz/2));

            var downSampledAverSpec = new double[DispPointsCnt];

            var interval = (hi - lo)/DispPointsCnt;
            for (int i = lo, j = 0; i < hi && j < DispPointsCnt; i += interval, j++)
            {
                downSampledAverSpec[j] = spetrum[i]/periodCnt;
            }
            return downSampledAverSpec;
            // todo _scaledBuffer may be not filled or overflow
        }

        public double[] DownSampleAndAverage(ISpectrum spec)
        {
            var lo = (int) (spec.Length()*StartFreqInMHz/(_samplingRateInMHz/2));
            var hi = (int) (spec.Length()*EndFreqInMHz/(_samplingRateInMHz/2));

            var downSampledAverSpec = new double[DispPointsCnt];

            var interval = (hi - lo)/DispPointsCnt;
            for (int i = lo, j = 0; i < hi && j < DispPointsCnt; i += interval, j++)
            {
                downSampledAverSpec[j] = spec.Power(i)/spec.PulseCount/spec.PulseCount;
            }
            return downSampledAverSpec;
            // todo _scaledBuffer may be not filled or overflow
        }
    }
}