using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpectroscopyVisualizer.Presenters {
    public class VerticalAxisView {
        public VerticalAxisView(Canvas canvas) {
            Canvas = canvas;
        }

        public Canvas Canvas { get; }

        public double Height => Canvas.ActualHeight;
        public double Width => Canvas.ActualWidth;

        public void DrawRuler(double minValue, double maxValue) {
            Canvas.Children.Clear();
            var height = Height;
            var width = Width;
            var depth = width/8;
            var baselinePts = new PointCollection(2) {new Point(width, 0), new Point(width, height)};
            var baseline = new Polyline {
                Points = baselinePts,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };
            Canvas.Children.Add(baseline);
            var interval = height/10;
            for (var i = 0; i < 11; i++) {
//                var points = new PointCollection(2) { new Point(width, i * interval), new Point(width-depth, i * interval) };
//                var line = new Polyline {
//                    Points = points,
//                    Stroke = new SolidColorBrush(Colors.White),
//                    StrokeThickness = 1
//                };
//                Canvas.Children.Add(line);


                var textBlock = new TextBlock();

                var mark = minValue + (maxValue - minValue)/10*i;

                var formatted = mark.ToString("#.00E+0");

                textBlock.Text = "" + formatted;

                textBlock.Foreground = new SolidColorBrush(Colors.White);

//                Canvas.SetLeft(textBlock, 5);

                Canvas.SetRight(textBlock, depth);

                Canvas.SetTop(textBlock, (10 - i)*interval - 10);

                Canvas.Children.Add(textBlock);
            }
            var label = new TextBlock {
                Text = "I\nN\nT\nE\nN\nS\nI\nT\nY\n/\nV^2",
                Foreground = new SolidColorBrush(Colors.White),
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(label, depth/2);
            Canvas.SetTop(label, height/3.5);
            Canvas.Children.Add(label);
        }
    }
}