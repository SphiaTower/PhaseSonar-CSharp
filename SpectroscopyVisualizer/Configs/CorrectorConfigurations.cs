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
        [Description("Specific Pts Range")] SpecificRange,
        [Description("Specific Freq Range")] SpecificFreqRange
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum OperationMode {
        [Description("Manual")] Manual,
        [Description("Single")] Single,
        [Description("Loop")] Loop
    }

    [Serializable]
    public class CorrectorConfigurations {
        private static CorrectorConfigurations _singleton;

        private CorrectorConfigurations(int zeroFillFactor, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType, PhaseType phaseType, double rangeStart, double rangeEnd,
            bool realSpec,bool lockDip,double lockDipFreqInMHz) {
            ZeroFillFactor = zeroFillFactor;
            CenterSpanLength = centerSpanLength;
            CorrectorType = correctorType;
            ApodizerType = apodizerType;
            PhaseType = phaseType;
            RangeStart = rangeStart;
            RangeEnd = rangeEnd;
            RealSpec = realSpec;
            LockDip = lockDip;
            LockDipFreqInMHz = lockDipFreqInMHz;
        }


        public int ZeroFillFactor { get; set; }
        public int CenterSpanLength { get; set; }
        public CorrectorType CorrectorType { get; set; }
        public ApodizerType ApodizerType { get; set; }
        public PhaseType PhaseType { get; set; }
        public double RangeStart { get; set; }
        public double RangeEnd { get; set; }
        public bool RealSpec { get; set; }
        public bool LockDip { get; set; }
        public double LockDipFreqInMHz { get; set; }
      
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
            ApodizerType apodizerType, PhaseType phaseType, double rangeStart, double rangeEnd,
            bool realPhase, bool lockDip, double lockDipFreqInMHz) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new CorrectorConfigurations(zeroFillFactor, centerSpanLength, correctorType, apodizerType,
                phaseType, rangeStart, rangeEnd,  realPhase,lockDip,lockDipFreqInMHz);
        }

    }
}