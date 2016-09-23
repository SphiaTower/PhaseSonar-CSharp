using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer.Presenters {
    public class EventLayer {
        private readonly Canvas _wavefromView;
        private Point _lastPoint;
        private bool _leftMouseDown;

        private EventLayer(Canvas canvas) {
            _wavefromView = canvas;
            Attach(canvas);
        }

        public double MouseDownStart { get; private set; }

        [NotNull]
        public static EventLayer Setup([NotNull] Canvas canvas) {
            return new EventLayer(canvas);
        }

        public event ZoomEventHandler ZoomEvent;
        public event MouseMoveEventHandler FollowTraceEvent;

        private int _lastTime=0;
        public void Attach([NotNull] Canvas canvas) {
            canvas.MouseLeftButtonDown += (sender, args) => {
                if (!_leftMouseDown) {
                    _lastPoint = args.GetPosition(canvas);
                    MouseDownStart = _lastPoint.X;
                    _leftMouseDown = true;
                }
            };
            canvas.MouseMove += (sender, args) => {
                var now = DateTime.Now.Millisecond;
                if (now - _lastTime <= 100) {
                    return;
                }
                var point = args.GetPosition(canvas);
                FollowTraceEvent?.Invoke(_lastPoint, point, _leftMouseDown);
                _lastPoint = point;
            };
            canvas.MouseLeftButtonUp += (sender, args) => {
                if (!_leftMouseDown) return;
                var zoomEnd = args.GetPosition(canvas).X;
                if (zoomEnd < MouseDownStart) {
                    var t = zoomEnd;
                    zoomEnd = MouseDownStart;
                    MouseDownStart = t;
                }

                ZoomEvent?.Invoke(MouseDownStart, zoomEnd, !(zoomEnd - MouseDownStart <= 12));

                _leftMouseDown = false;
            };
            canvas.MouseRightButtonDown += (sender, args) => { UndoEvent?.Invoke(); };
            canvas.MouseDown += (sender, args) => {
                if (args.ChangedButton == MouseButton.Middle) {
                    AdjustYAxisEvent?.Invoke();
                }
            };
        }

        public event MouseButtonEventHandler MouseRightButtonDown {
            add { _wavefromView.MouseRightButtonDown += value; }
            remove { _wavefromView.MouseRightButtonDown -= value; }
        }

        public event GeneralEventHandler AdjustYAxisEvent;
        public event GeneralEventHandler UndoEvent;
    }

    public delegate void GeneralEventHandler();

    public delegate void MouseMoveEventHandler(Point last, Point curr, bool mouseDown);

    public delegate void ZoomEventHandler(double zoomStart, double zoomEnd, bool valid);
}