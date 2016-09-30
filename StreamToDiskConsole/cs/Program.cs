//====================================================================================================
//
// Title:
//        Stream To Disk Console
//
// Description:
//      The Application demonstrates the programmatic usage of .Net API for NI-Scope.
//      The application acquires data from a channel and saves it onto a file in the local disk.
//      The waveform stored in the file can be loaded by another shipping example titled "Stream To Disk".
//         
//=================================================================================================

using System;
using System.IO;
using NationalInstruments.ModularInstruments.NIScope;

namespace NationalInstruments.Examples.StreamToDiskConsole {
    internal class Program {
        private static NIScope _scopeSession;

        private static void Main() {
           /* var input = new ConsoleInput();
            input.AskUser();

            var sampler = Sampler.FromBinder(input);
            var analogWaveformCollection = sampler.Sample();
            sampler.Release();
            var ouputStream = DiskWriter.GetOuputStream();
            DiskWriter.SaveBinaryWaveform(ouputStream, analogWaveformCollection);
            Console.ReadKey();*/
/*

            string deviceName, channelList;
            double range, sampleRateMin;
            long recordLengthMin;
            FileStream outputFileStream;
            AnalogWaveformCollection<double> waveforms = null;
            Exception thrownException = null;

            GetInputParameters(out deviceName, out channelList, out range, out sampleRateMin, out recordLengthMin, out outputFileStream);

            try
            {
                InitializeSession(deviceName);

                _scopeSession.Channels[channelList].Enabled = true;
                _scopeSession.Channels[channelList].Range = range;
                _scopeSession.Acquisition.SampleRateMin = sampleRateMin;
                _scopeSession.Acquisition.NumberOfPointsMin = recordLengthMin;

                _scopeSession.Measurement.Initiate();
                waveforms = _scopeSession.Channels[channelList].Measurement.FetchDouble(PrecisionTimeSpan.Zero, recordLengthMin, waveforms);

                new BinaryFormatter().Serialize(outputFileStream, waveforms);
            }
            catch (SecurityException ex)
            {
                Console.WriteLine("Unable to serialize the data: ");
                thrownException = ex;
            }
            catch (SerializationException ex)
            {
                Console.WriteLine("Unable to serialize the data: ");
                thrownException = ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured in NI-Scope: ");
                thrownException = ex;
            }
            finally
            {
                if (_scopeSession != null)
                {
                    _scopeSession.Close();
                    _scopeSession = null;
                }
                if (outputFileStream != null)
                {
                    outputFileStream.Close();
                }
                if (thrownException != null)
                {
                    Console.WriteLine(thrownException.Message);
                }
                else
                {
                    Console.WriteLine("Successfully saved acquired data to \"" + outputFileStream.Name + "\"");
                }
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }*/
        }

        private static void InitializeSession(string deviceName) {
            _scopeSession = new NIScope(deviceName, false, false);
            _scopeSession.DriverOperation.Warning += DriverOperation_Warning;
        }

        private static void DriverOperation_Warning(object sender, ScopeWarningEventArgs e) {
            Console.WriteLine(e.Text);
        }

        private static void GetInputParameters(out string deviceName, out string channelList, out double range,
            out double SampleRateMin, out long recordLengthMin, out FileStream outputFileStream) {
            outputFileStream = null;

            Console.WriteLine("Provide input parameter values for the acquisition:");
            Console.WriteLine("Press Enter to accept the default values for the parameters.");
            Console.WriteLine();

            deviceName = GetInputString("Device Name", "Dev1");
            channelList = GetInputString("Channel List", "0");

            while (!double.TryParse(GetInputString("Range", "10"), out range)) {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            while (!double.TryParse(GetInputString("Minimum Sample Rate", "1e6"), out SampleRateMin)) {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            while (!long.TryParse(GetInputString("Minimum Record Length", "1000"), out recordLengthMin)) {
                Console.WriteLine("The entered value is not in the correct format. Please try again:");
            }
            while (outputFileStream == null) {
                try {
                    // Folder location.
                    var directoryPath = @"C:\waveform";
                    if (!Directory.Exists(directoryPath)) {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = new FileInfo(Path.Combine(directoryPath, @"waveform1.txt"));
                    if (!fileName.Exists) {
                        fileName.Create().Close();
                    } else {
                        File.WriteAllText(fileName.ToString(), string.Empty);
                    }
                    outputFileStream = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.ReadWrite);
                } catch (Exception e) {
                    Console.WriteLine("Unable to create a file stream with the given path:");
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private static string GetInputString(string prompt, string defaultValue) {
            Console.Write(prompt + " [" + defaultValue + "]:");
            var inputString = Console.ReadLine();

            if (string.IsNullOrEmpty(inputString))
                return defaultValue;
            return inputString;
        }
    }
}