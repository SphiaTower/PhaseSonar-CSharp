using System;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class SliceConfigurations {
        private static SliceConfigurations _singleton;

        private SliceConfigurations(int pointsBeforeCrest, bool crestAtCenter, double crestAmplitudeThreshold,
            RulerType rulerType) {
            PointsBeforeCrest = pointsBeforeCrest;
            CrestAtCenter = crestAtCenter;
            CrestAmplitudeThreshold = crestAmplitudeThreshold;
            RulerType = rulerType;
        }

        public int PointsBeforeCrest { get; set; }
        public bool CrestAtCenter { get; set; }
        public double CrestAmplitudeThreshold { get; set; }
        public RulerType RulerType { get; set; }

        public static void Initialize(int pointsBeforeCrest, bool crestAtCenter, double crestAmplitudeThreshold,
            RulerType rulerType) {
            _singleton = new SliceConfigurations(pointsBeforeCrest, crestAtCenter, crestAmplitudeThreshold, rulerType);
        }

        internal void Register(SliceConfigurations sliceConfigurations) {
            _singleton = sliceConfigurations;
        }

        public static SliceConfigurations Get() {
            return _singleton;
        }

        public void Bind(Control pointsBeforeCrest, Control crestAmpThreshold, Control rulerType) {
            pointsBeforeCrest.DataContext = this;
            crestAmpThreshold.DataContext = this;
            rulerType.DataContext = this;
        }
    }
}