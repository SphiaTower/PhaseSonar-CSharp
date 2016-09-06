using System;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class SliceConfigurations {
        private static SliceConfigurations _singleton;

        private SliceConfigurations(int pointsBeforeCrest, bool centreSlice, double crestAmplitudeThreshold) {
            PointsBeforeCrest = pointsBeforeCrest;
            CentreSlice = centreSlice;
            CrestAmplitudeThreshold = crestAmplitudeThreshold;
        }

        public int PointsBeforeCrest { get; set; }
        public bool CentreSlice { get; set; }
        public double CrestAmplitudeThreshold { get; set; }

        public static void Initialize(int pointsBeforeCrest, bool centreSlice, double crestAmplitudeThreshold) {
            _singleton = new SliceConfigurations(pointsBeforeCrest, centreSlice, crestAmplitudeThreshold);
        }

        internal void Register(SliceConfigurations sliceConfigurations) {
            _singleton = sliceConfigurations;
        }

        public static SliceConfigurations Get() {
            return _singleton;
        }

        public void Bind(Control pointsBeforeCrest, Control crestAmpThreshold) {
            pointsBeforeCrest.DataContext = this;
            crestAmpThreshold.DataContext = this;
        }
    }
}