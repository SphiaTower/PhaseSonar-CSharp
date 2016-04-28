using System;
using System.Windows.Controls;
using FTIR.Utils;

namespace Shokouki.Configs
{
    public class SamplingConfigs
    {
        private static SamplingConfigs _singleton;

        private SamplingConfigs(string deviceName, int channel, double samplingRate, long recordLength, double range)
        {
            Toolbox.RequireNonNull(deviceName, "device name is null");
            Toolbox.RequireRange(channel, chan => chan >= 0 && chan <= 2);
            Toolbox.RequireRange(samplingRate, rate => rate > 0);
            Toolbox.RequireRange(recordLength, length => length > 0);
            Toolbox.RequireRange(range, r => r > 0);

            SamplingRate = samplingRate;
            Channel = channel;
            DeviceName = deviceName;
            Range = range;
            RecordLength = recordLength;
        }

        public void Bind(Control deviceName,Control channel,Control samplingRate, Control recordLength,Control range)
        {
            deviceName.DataContext = this;
            channel.DataContext = this;
            samplingRate.DataContext = this;
            recordLength.DataContext = this;
            range.DataContext = this;
        }

        public string DeviceName { get; set; }
        public int Channel { get; set; }
        public double SamplingRate { get; set; }
        public long RecordLength { get; set; }
        public double Range { get; set; }

        public static void Initialize(string deviceName, int channel, double samplingRate, long recordLength,
            double range)
        {
            if (_singleton != null)
            {
                throw new Exception("SamplingConfigs already init ");
            }
            _singleton = new SamplingConfigs(deviceName, channel, samplingRate, recordLength, range);
        }

        public static SamplingConfigs Get()
        {
            return _singleton;
        }
    }
}