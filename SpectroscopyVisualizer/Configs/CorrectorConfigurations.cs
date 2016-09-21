using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs {
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CorrectorType {
        [Description("None")] Fake,
        [Description("Linear(TODO)")] LinearMertz,
        [Description("Mertz")] Mertz
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ApodizerType {
        [Description("None")] Fake,
        [Description("Triangular")] Triangular
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RulerType {
        [Description("Minimum Length")] MinLength,
        [Description("Average Length")] AverageLength,
        [Description("Fixed Length")] FixLength
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PhaseType {
        [Description("Full Range")] FullRange,
        [Description("Center Interpolation")] CenterInterpolation,
        [Description("Old Wrong Method")] OldCenterInterpolation,
        [Description("Specified Range")] SpecifiedRange
    }

    [Serializable]
    public class CorrectorConfigurations {
        private static CorrectorConfigurations _singleton;

        private CorrectorConfigurations(int zeroFillFactor, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType,PhaseType phaseType,int rangeStart, int rangeEnd) {
            ZeroFillFactor = zeroFillFactor;
            CenterSpanLength = centerSpanLength;
            CorrectorType = correctorType;
            ApodizerType = apodizerType;
            PhaseType = phaseType;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
        }


        public int ZeroFillFactor { get; set; }
        public int CenterSpanLength { get; set; }
        public CorrectorType CorrectorType { get; set; }
        public ApodizerType ApodizerType { get; set; }
        public PhaseType PhaseType { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public static CorrectorConfigurations Get() {
            return _singleton;
        }

        public static void Register(CorrectorConfigurations configuration) {
            _singleton = configuration;
        }

        public static void Initialize(int zeroFillFactory, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType,PhaseType phaseType, int rangeStart, int rangeEnd) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new CorrectorConfigurations(zeroFillFactory, centerSpanLength, correctorType, apodizerType, phaseType,rangeStart,rangeEnd);
        }

        public void Bind(Control tbZeroFillFactor, Control tbCenterSpanLength, Control cbCorrectorType,
            Control cbApodizationType,Control cbPhaseType,Control tbRangeStart, Control tbRangeEnd) {
            tbZeroFillFactor.DataContext = this;
            tbCenterSpanLength.DataContext = this;
            cbCorrectorType.DataContext = this;
            cbApodizationType.DataContext = this;
            cbPhaseType.DataContext = this;
            tbRangeStart.DataContext = this;
            tbRangeEnd.DataContext = this;
        }
    }
}