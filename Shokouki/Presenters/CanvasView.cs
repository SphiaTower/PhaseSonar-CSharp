using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shokouki.Presenters
{
    public class CanvasView
    {
        private readonly List<Polyline> _lines = new List<Polyline>();
        private readonly List<Polyline> _waveforms = new List<Polyline>(2);

        public CanvasView(Canvas canvas)
        {
            Canvas = canvas;
        }

        public Canvas Canvas { get; }

        public double ScopeHeight => Canvas.ActualHeight;
        public double ScopeWidth => Canvas.ActualWidth;

        public void Invoke(Action action)
        {
            Canvas.Dispatcher.Invoke(action);
        }

        public void InvokeAsync(Action action)
        {
            Canvas.Dispatcher.InvokeAsync(action);
        }

        public void DrawWaveform(PointCollection pointCollection, Color color)
        {
            var line = DrawLineBase(pointCollection, color);
            _waveforms.Add(line);
        }

        private Polyline DrawLineBase(PointCollection pointCollection, Color color)
        {
            var line = new Polyline
            {
                Points = pointCollection,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1
            };
            Canvas.Children.Add(line);
            return line;
        }

        public void DrawLine(PointCollection pointCollection, Color color)
        {
            var line = DrawLineBase(pointCollection, color);
            _lines.Add(line);
        }

        public void DrawWaveform(PointCollection pointCollection)
        {
            DrawWaveform(pointCollection, Colors.White);
        }

        public void DrawGrid()
        {
            var width = ScopeWidth;
            var height = ScopeHeight;
            var xInterval = width/10;

            for (var i = 0; i <= 10; i++)
            {
                var linePts = new PointCollection(2) {new Point(xInterval*i, 0), new Point(xInterval*i, height)};
                var line = new Polyline
                {
                    Points = linePts,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    StrokeDashArray = new DoubleCollection() { 1,1}
                };
                Canvas.Children.Add(line);
            }
            var yInterval = height/10;
            for (var i = 0; i <= 10; i++)
            {
                var linePts = new PointCollection(2) {new Point(0, yInterval*i), new Point(width, yInterval*i)};
                var line = new Polyline
                {
                    Points = linePts,
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    StrokeDashArray = new DoubleCollection() { 1, 1 }
                };
                Canvas.Children.Add(line);
            }
        }

        public void ClearWaveform()
        {
            foreach (var waveform in _waveforms)
            {
                Canvas.Children.Remove(waveform);
            }
//            Canvas.Children.ClearWaveform();
        }

        public void ClearLine()
        {
            foreach (var polyline in _lines)
            {
                Canvas.Children.Remove(polyline);
            }
        }
    }
}