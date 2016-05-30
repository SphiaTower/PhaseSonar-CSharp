using System;
using NationalInstruments.ModularInstruments.NIScope;

namespace NationalInstruments.Examples.StreamToDiskConsole
{
    public class Sampler
    {
        private string _channelList;

        private string _deviceName;
        private double _range;

        private long _recordLengthMin;
        private double _sampleRateMin;

        public Sampler(string deviceName, string channelList, double range, double sampleRateMin, long recordLengthMin)
        {
            try
            {
                DeviceName = deviceName;
                ChannelList = channelList;
                Range = range;
                SampleRateMin = sampleRateMin;
                RecordLengthMin = recordLengthMin;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured in NI-Scope: " + e.Message);
            }
        }

        public NIScope ScopeSession { get; private set; }

        public string ChannelList
        {
            get { return _channelList; }
            set
            {
                _channelList = value;
                ScopeSession.Channels[_channelList].Enabled = true;
            }
        }

        public double Range
        {
            get { return _range; }
            set
            {
                _range = value;
                ScopeSession.Channels[_channelList].Range = value;
            }
        }

        public long RecordLengthMin
        {
            get { return _recordLengthMin; }
            set
            {
                _recordLengthMin = value;
                ScopeSession.Acquisition.NumberOfPointsMin = value;
            }
        }

        public double SampleRateMin
        {
            get { return _sampleRateMin; }
            set
            {
                _sampleRateMin = value;
                ScopeSession.Acquisition.SampleRateMin = value;
            }
        }

        public string DeviceName
        {
            private get { return _deviceName; }
            set
            {
                _deviceName = value;
                ScopeSession = new NIScope(value, false, false);
                ScopeSession.DriverOperation.Warning += DriverOperation_Warning;
            }
        }

        public double[] Buffer { get; private set; }

        public static Sampler FromBinder(IParamBinder binder)
        {
            return new Sampler(binder.DeviceName, binder.ChanelList, binder.Range, binder.SampleRate,
                binder.RecordLength);
        }


        private static void DriverOperation_Warning(object sender, ScopeWarningEventArgs e)
        {
            Console.WriteLine(e.Text);
        }

        public AnalogWaveformCollection<double> Sample()
        {
            AnalogWaveformCollection<double> waveforms = null;
            ScopeSession.Measurement.Initiate();
            waveforms = ScopeSession.Channels[_channelList].Measurement.FetchDouble(PrecisionTimeSpan.FromSeconds(-1),
                _recordLengthMin, waveforms);
            return waveforms;
        }

        public ScopeAcquisitionStatus Status()
        {
            return ScopeSession.Measurement.Status(); // todo bug of multi-instances
        }

        public double[] Retrieve(int channel =0) //todo! hard coded!
        {
            return Sample()[channel].GetScaledData();
        }

        public void RetrieveToBuffer(int channel = 0)
        {
            if (Buffer == null)
            {
                Buffer = new double[_recordLengthMin];
            }
            Sample()[channel].GetScaledData(0, Buffer.Length, Buffer, 0);
        }

        public void Release()
        {
            ScopeSession.Close();
            ScopeSession = null;
        }

        public interface IParamBinder
        {
            string DeviceName { get; set; }
            string ChanelList { get; set; }
            double Range { get; set; }
            double SampleRate { get; set; }
            long RecordLength { get; set; }
        }
    }
}