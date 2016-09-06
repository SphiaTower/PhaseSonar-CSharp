using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

namespace NationalInstruments.Examples.StreamToDiskConsole {
    public class DiskWriter {
        public static FileStream GetOuputStream(string directory = @"C:\waveform",
            string filename = @"waveform1.txt") {
            FileStream outputFileStream = null;
            while (outputFileStream == null) {
                try {
                    // Folder location.
                    var directoryPath = directory;
                    if (!Directory.Exists(directoryPath)) {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = new FileInfo(Path.Combine(directoryPath, filename));
                    if (!fileName.Exists) {
                        fileName.Create().Close();
                    }
                    else {
                        File.WriteAllText(fileName.ToString(), string.Empty);
                    }
                    outputFileStream = new FileStream(fileName.ToString(), FileMode.Create, FileAccess.ReadWrite);
                }
                catch (Exception e) {
                    Console.WriteLine("Unable to create a file stream with the given path:");
                    Console.WriteLine(e.Message);
                }
            }
            return outputFileStream;
        }

        public static void SaveBinaryWaveform(FileStream outputFileStream, AnalogWaveformCollection<double> waveforms) {
            try {
                new BinaryFormatter().Serialize(outputFileStream, waveforms);
                Console.WriteLine("Successfully saved acquired data to \"" + outputFileStream.Name + "\"");
            }
            catch (SecurityException ex) {
                Console.WriteLine("Unable to serialize the data: ");
            }
            catch (SerializationException ex) {
                Console.WriteLine("Unable to serialize the data: ");
            }
            finally {
                outputFileStream.Close();
            }
        }
    }
}