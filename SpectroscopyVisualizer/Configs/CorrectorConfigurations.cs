using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace SpectroscopyVisualizer.Configs
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CorrectorType
    {
        [Description("None")] Fake,
        [Description("Linear(TODO)")] LinearMertz,
        [Description("Mertz")] Mertz
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ApodizerType
    {
        [Description("None")] Fake,
        [Description("Triangular")] Triangular
    }

    [Serializable]
    public class CorrectorConfigurations
    {
        private static CorrectorConfigurations _singleton;

        private CorrectorConfigurations(int zeroFillFactor, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType)
        {
            ZeroFillFactor = zeroFillFactor;
            CenterSpanLength = centerSpanLength;
            CorrectorType = correctorType;
            ApodizerType = apodizerType;
        }


        public int ZeroFillFactor { get; set; }
        public int CenterSpanLength { get; set; }
        public CorrectorType CorrectorType { get; set; }
        public ApodizerType ApodizerType { get; set; }

        public static CorrectorConfigurations Get()
        {
            return _singleton;
        }

        public static void Register(CorrectorConfigurations configuration)
        {
            _singleton = configuration;
        }

        public static void Initialize(int zeroFillFactory, int centerSpanLength, CorrectorType correctorType,
            ApodizerType apodizerType)
        {
            if (_singleton != null)
            {
                throw new Exception("environment already init");
            }
            _singleton = new CorrectorConfigurations(zeroFillFactory, centerSpanLength, correctorType, apodizerType);
        }

        public void Bind(Control tbZeroFillFactor, Control tbCenterSpanLength, Control cbCorrectorType,
            Control cbApodizationType)
        {
            tbZeroFillFactor.DataContext = this;
            tbCenterSpanLength.DataContext = this;
            cbCorrectorType.DataContext = this;
            cbApodizationType.DataContext = this;
        }
    }
}