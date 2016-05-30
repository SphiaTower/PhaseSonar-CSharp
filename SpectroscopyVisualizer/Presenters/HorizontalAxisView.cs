using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpectroscopyVisualizer.Presenters {
    public class HorizontalAxisView {
        public Canvas Canvas { get; }
        public HorizontalAxisView(Canvas canvas) {
            Canvas = canvas;
        }

        public double Height => Canvas.ActualHeight;
        public double Width => Canvas.ActualWidth;

        public void DrawRuler(double startFreqInMHz, double endFreqInMHz)
        {
            Canvas.Children.Clear();
            var width = Width;
            var depth = Height / 8;
            var baselinePts = new PointCollection(2) { new Point(0, 0), new Point(width, 0) };
            var baseline = new Polyline()
            {
                Points = baselinePts,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };
            Canvas.Children.Add(baseline);
            var interval = width / 10;
            for(int i = 0; i < 11; i++) {
//                var points = new PointCollection(2) {new Point(i*interval, 0), new Point(i*interval, depth)};
//                var line = new Polyline {
//                    Points = points,
//                    Stroke = new SolidColorBrush(Colors.White),
//                    StrokeThickness = 1
//                };
//                Canvas.Children.Add(line);


                var textBlock = new TextBlock();

                var mark = Math.Round(startFreqInMHz+(endFreqInMHz-startFreqInMHz) / 10*i,3);

                textBlock.Text = ""+ mark.ToString("F3");

                textBlock.Foreground = new SolidColorBrush(Colors.White);

                Canvas.SetLeft(textBlock, i*interval-15 );

                Canvas.SetTop(textBlock, depth);

                Canvas.Children.Add(textBlock);
            }
            var label = new TextBlock
            {
                Text = "FREQUENCY/MHz",
                Foreground = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(label, width/2.5);
            Canvas.SetBottom(label, depth/2);
            Canvas.Children.Add(label);
        }

    }
}
