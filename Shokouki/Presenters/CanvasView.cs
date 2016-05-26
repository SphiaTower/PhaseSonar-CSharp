using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Shokouki.Presenters
{
    public class CanvasView : IScopeView
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

        public void DrawLine(PointCollection pointCollection, Color color)
        {
            var line = new Polyline
            {
                Points = pointCollection,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1
            };
            Canvas.Children.Add(line);
        }

        public double ScopeHeight => Canvas.ActualHeight;
        public double ScopeWidth => Canvas.ActualWidth;

        public void DrawLine(PointCollection pointCollection)
        {
            DrawLine(pointCollection, Colors.White);
        }


        public void Clear()
        {
            Canvas.Children.Clear();
        }
    }
}