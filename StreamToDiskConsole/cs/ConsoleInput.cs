using System;

namespace NationalInstruments.Examples.StreamToDiskConsole
{
    public class ConsoleInput : Sampler.IParamBinder
    {
      

        public void AskUser()
        {
            Console.WriteLine("Provide input parameter values for the acquisition:");
            Console.WriteLine("Press Enter to accept the default values for the parameters.");
            Console.WriteLine();

            DeviceName = GetInputString("Device Name", "Dev1");
            ChanelList = GetInputString("Channel List", "0");

            double range;
            while (!double.TryParse(GetInputString("Range", "10"), out range))
            {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            Range = range;
            double sampleRate;
            while (!double.TryParse(GetInputString("Minimum Sample Rate", "1e6"), out sampleRate))
            {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            SampleRate = sampleRate;
            long recordLength;
            while (!long.TryParse(GetInputString("Minimum Record Length", "1000"), out recordLength))
            {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            RecordLength = recordLength;
            // SaveToFile();
            Console.WriteLine();
            Console.WriteLine();
        }

        public void SetDefault()
        {
            DeviceName = "Dev2";
            ChanelList = "0";
            Range = 10;
            SampleRate = 100e6;
            RecordLength = (long) 1e6;
        }

        private static string GetInputString(string prompt, string defaultValue)
        {
            Console.Write(prompt + " [" + defaultValue + "]:");
            var inputString = Console.ReadLine();

            if (string.IsNullOrEmpty(inputString))
                return defaultValue;
            return inputString;
        }

        public string DeviceName { get; set; }
        public string ChanelList { get; set; }
        public double Range { get; set; }
        public double SampleRate { get; set; }
        public long RecordLength { get; set; }
    }
}