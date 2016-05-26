using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;

namespace FTIR.Utils
{
    public class Toolbox
    {
        public static double[] Read(string path)
        {
            var lines = File.ReadAllLines(path);
            var data = new double[lines.Length];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = double.Parse(lines[i]);
            }
            lines = null;
            return data;
        }

        public static void WriteData<T>(string path, [NotNull] T[] data)
        {
            var contents = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                contents[i] = data[i].ToString();
            }
            File.WriteAllLines(path, contents);
        }

        public static void SerializeData<T>(string path, [NotNull] T data)
        {
            var file = new FileInfo(path);
            var fileStream = file.OpenWrite();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, data);
        }

        public static T DeserializeData<T>(string path)
        {
            var file = new FileInfo(path);
            var fileStream = file.OpenRead();
            var binaryFormatter = new BinaryFormatter();
            return (T) binaryFormatter.Deserialize(fileStream);
        }

        public static void RequireNonNull(object arg, string name)
        {
            if (arg == null)
            {
                throw new NullReferenceException(name);
            }
        }

        public static void RequireNonNull(object arg)
        {
            if (arg == null)
            {
                throw new NullReferenceException(nameof(arg));
            }
        }

        public static void RequireLargerThan(object arg, int floor)
        {
            if (Comparer.Default.Compare(arg, floor) <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " must > " + floor);
            }
        }

        public static void RequireRange(int arg, Func<int, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }

        public static void RequireRange(double arg, Func<double, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }

        public static void RequireRange(long arg, Func<long, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }
    }
}