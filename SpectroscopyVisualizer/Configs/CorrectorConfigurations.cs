using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs {
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CorrectorType {
        [Description("FFT Only")] Fake,
        [Description("Mertz")] Mertz
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ApodizerType {
        [Description("None")] Fake,
        [Description("Triangular")] Triangular,
        [Description("Hann")] Hann,
        [Description("Hamming")] Hamming,
        [Description("Cosine")] Cosine
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
        [Description("Classic Method")] OldCenterInterpolation,
        [Description("Specified Range")] SpecifiedRange,
        [Description("Specified Freq Range")] SpecifiedFreqRange
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SaveType {
        [Description("Complex")] Complex,
        [Description("Magnitude")] Magnitude,
        [Description("Intensity")] Intensity,
        [Description("Phase")] Phase,
        [Description("Unwrapped Phase")] UnwrappedPhase,
        [Description("Real")] Real,
        [Description("Imaginary")] Imaginary
    }

    [Serializable]
    public class CorrectorConfigurations {
        private static CorrectorConfigurations _singleton;

        private CorrectorConfigurations(int zeroFillFactor, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType, PhaseType phaseType, double rangeStart, double rangeEnd, bool autoFlip,
            bool realSpec) {
            ZeroFillFactor = zeroFillFactor;
            CenterSpanLength = centerSpanLength;
            CorrectorType = correctorType;
            ApodizerType = apodizerType;
            PhaseType = phaseType;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            AutoFlip = autoFlip;
            RealSpec = realSpec;
        }


        public int ZeroFillFactor { get; set; }
        public int CenterSpanLength { get; set; }
        public CorrectorType CorrectorType { get; set; }
        public ApodizerType ApodizerType { get; set; }
        public PhaseType PhaseType { get; set; }
        public double RangeStart { get; set; }
        public double RangeEnd { get; set; }
        public bool AutoFlip { get; set; }
        public bool RealSpec { get; set; }

        public static CorrectorConfigurations Get() {
            return _singleton;
        }

        public static void Register(CorrectorConfigurations configuration) {
            if (_singleton == null) {
                _singleton = configuration;
            } else {
                ConfigsHolder.CopyTo(configuration, _singleton);
            }
        }

        public static void Initialize(int zeroFillFactor, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType, PhaseType phaseType, double rangeStart, double rangeEnd, bool autoFlip,
            bool realPhase) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new CorrectorConfigurations(zeroFillFactor, centerSpanLength, correctorType, apodizerType,
                phaseType, rangeStart, rangeEnd, autoFlip, realPhase);
        }

        public void Bind(Control tbZeroFillFactor, Control tbCenterSpanLength, Control cbCorrectorType,
            Control cbApodizationType, Control cbPhaseType, Control tbRangeStart, Control tbRangeEnd, Control ckAutoFlip,
            Control ckRealSpec) {
            tbZeroFillFactor.DataContext = this;
            tbCenterSpanLength.DataContext = this;
            cbCorrectorType.DataContext = this;
            cbApodizationType.DataContext = this;
            cbPhaseType.DataContext = this;
            tbRangeStart.DataContext = this;
            tbRangeEnd.DataContext = this;
            ckAutoFlip.DataContext = this;
            ckRealSpec.DataContext = this;
        }
    }
}