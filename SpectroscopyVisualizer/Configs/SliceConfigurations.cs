using System;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class SliceConfigurations {
        private static SliceConfigurations _singleton;

        private SliceConfigurations(int pointsBeforeCrest, bool crestAtCenter, double crestAmplitudeThreshold, RulerType rulerType, bool autoAdjust, bool findAbs, int fixedLength, bool reference) {
            PointsBeforeCrest = pointsBeforeCrest;
            CrestAtCenter = crestAtCenter;
            CrestAmplitudeThreshold = crestAmplitudeThreshold;
            RulerType = rulerType;
            AutoAdjust = autoAdjust;
            FindAbsoluteValue = findAbs;
            FixedLength = fixedLength;
            Reference = reference;
        }

        public int PointsBeforeCrest { get; set; }
        public bool CrestAtCenter { get; set; }
        public double CrestAmplitudeThreshold { get; set; }
        public RulerType RulerType { get; set; }
        public bool AutoAdjust { get; set; }
        public bool FindAbsoluteValue { get; set; }
        public int FixedLength { get; set; }
        public bool Reference { get; set; }

        public static void Initialize(int pointsBeforeCrest, bool crestAtCenter, double crestAmplitudeThreshold,
            RulerType rulerType, bool autoAdjust, bool findAbs, int fixedLength,bool reference) {
            _singleton = new SliceConfigurations(pointsBeforeCrest, crestAtCenter, crestAmplitudeThreshold, rulerType,
                autoAdjust, findAbs, fixedLength,reference);
        }

        public static void Register(SliceConfigurations sliceConfigurations) {
            if (_singleton == null) {
                _singleton = sliceConfigurations;
            } else {
                ConfigsHolder.CopyTo(sliceConfigurations, _singleton);
            }
        }

        public static SliceConfigurations Get() {
            return _singleton;
        }

        public void Bind(Control pointsBeforeCrest, Control crestAmpThreshold, Control rulerType, Control autoAdjust,
            Control findAbs, Control fixedLength,Control reference) {
            pointsBeforeCrest.DataContext = this;
            crestAmpThreshold.DataContext = this;
            rulerType.DataContext = this;
            autoAdjust.DataContext = this;
            findAbs.DataContext = this;
            fixedLength.DataContext = this;
            reference.DataContext = this;
        }
    }
}