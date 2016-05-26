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
        public CanvasView(Canvas canvas)
        {
            Canvas = canvas;
        }

        public Canvas Canvas { get; }

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

        private Polyline DrawLineBase(PointCollection pointCollection, Color color) {
            var line = new Polyline {
                Points = pointCollection,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1
            };
            Canvas.Children.Add(line);
            return line;
        }

        public void DrawLine(PointCollection pointCollection, Color color) {
            var line = DrawLineBase(pointCollection, color);
            _lines.Add(line);
        }
        private List<Polyline> _waveforms = new List<Polyline>(2);
        private List<Polyline> _lines = new List<Polyline>();

        public double ScopeHeight => Canvas.ActualHeight;
        public double ScopeWidth => Canvas.ActualWidth;

        public void DrawWaveform(PointCollection pointCollection)
        {
            DrawWaveform(pointCollection, Colors.White);
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