using System.Windows.Controls;

namespace Shokouki.Configs
{
    public class SliceConfigs
    {
        private static SliceConfigs _singleton;

        private SliceConfigs(int pointsBeforeCrest, bool centreSlice, double crestAmplitudeThreshold)
        {
            PointsBeforeCrest = pointsBeforeCrest;
            CentreSlice = centreSlice;
            CrestAmplitudeThreshold = crestAmplitudeThreshold;
        }

        public int PointsBeforeCrest { get; set; }
        public bool CentreSlice { get; set; }
        public double CrestAmplitudeThreshold { get; set; }

        public static void Initialize(int pointsBeforeCrest, bool centreSlice, double crestAmplitudeThreshold)
        {
            _singleton = new SliceConfigs(pointsBeforeCrest, centreSlice, crestAmplitudeThreshold);
        }

        public static SliceConfigs Get()
        {
            return _singleton;
        }

        public void Bind(Control pointsBeforeCrest, Control crestAmpThreshold)
        {
            pointsBeforeCrest.DataContext = this;
            crestAmpThreshold.DataContext = this;
        }
    }
}