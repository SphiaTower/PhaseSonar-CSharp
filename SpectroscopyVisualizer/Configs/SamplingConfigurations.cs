﻿using System;
using System.Windows.Controls;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class SamplingConfigurations {
        private static SamplingConfigurations _singleton;

        private SamplingConfigurations(string deviceName, int channel, double samplingRateInMHz, long recordLengthInM,
            double range) {
            Toolbox.RequireNonNull(deviceName, "device name is null");
            Toolbox.RequireRange(channel, chan => chan >= 0 && chan <= 2);
            Toolbox.RequireRange(samplingRateInMHz, rate => rate > 0);
            Toolbox.RequireRange(recordLengthInM, length => length > 0);
            Toolbox.RequireRange(range, r => r > 0);

            SamplingRateInMHz = samplingRateInMHz;
            Channel = channel;
            DeviceName = deviceName;
            Range = range;
            RecordLengthInM = recordLengthInM;
        }

        public string DeviceName { get; set; }
        public int Channel { get; set; }
        public double SamplingRateInMHz { get; set; }
        public double SamplingRate => SamplingRateInMHz*1e6;
        public long RecordLengthInM { get; set; }
        public long RecordLength => (long) (RecordLengthInM*1e6);
        public double Range { get; set; }

        public static void Register(SamplingConfigurations samplingConfigurations) {
            if (_singleton == null) {
                _singleton = samplingConfigurations;
            } else {
                ConfigsHolder.CopyTo(samplingConfigurations, _singleton);
            }
        }

  
        public static void Initialize(string deviceName, int channel, double samplingRateInMHz, long recordLengthInM,
            double range) {
            if (_singleton != null) {
                throw new Exception("SamplingConfigs already init ");
            }
            _singleton = new SamplingConfigurations(deviceName, channel, samplingRateInMHz, recordLengthInM, range);
        }

        public static SamplingConfigurations Get() {
            return _singleton;
        }
    }
}