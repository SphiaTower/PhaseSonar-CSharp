using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;

namespace PhaseSonar.Utils
{
    /// <summary>
    ///     A utility toolbox.
    /// </summary>
    public class Toolbox
    {
        /// <summary>
        ///     Read data from a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Write data to a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void WriteData<T>(string path, [NotNull] T[] data)
        {
            var contents = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                contents[i] = data[i].ToString();
            }
            File.WriteAllLines(path, contents);
        }

        /// <summary>
        ///     Write string array to a given path line by line.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public static void WriteStringArray(string path, [NotNull] string[] data)
        {
            File.WriteAllLines(path, data);
        }

        /// <summary>
        ///     Serialize data into binary format to a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void SerializeData<T>(string path, [NotNull] T data)
        {
            var file = new FileInfo(path);
            var fileStream = file.OpenWrite();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, data);
        }

        /// <summary>
        ///     Deserialize data in binary format from a given path.
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeserializeData<T>(string path)
        {
            var file = new FileInfo(path);
            var fileStream = file.OpenRead();
            var binaryFormatter = new BinaryFormatter();
            return (T) binaryFormatter.Deserialize(fileStream);
        }

        /// <summary>
        ///     Require the argument not null.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="name"></param>
        /// <exception cref="NullReferenceException"></exception>
        public static void RequireNonNull(object arg, string name)
        {
            if (arg == null)
            {
                throw new NullReferenceException(name);
            }
        }

        /// <summary>
        ///     Require the argument not null.
        /// </summary>
        /// <param name="arg"></param>
        /// <exception cref="NullReferenceException"></exception>
        public static void RequireNonNull(object arg)
        {
            if (arg == null)
            {
                throw new NullReferenceException(nameof(arg));
            }
        }


        /// <summary>
        ///     Require arg to satisfy a function.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="func"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void RequireRange(int arg, Func<int, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }

        /// <summary>
        ///     Require arg to satisfy a function.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="func"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void RequireRange(double arg, Func<double, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }

        /// <summary>
        ///     Require arg to satisfy a function.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="func"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void RequireRange(long arg, Func<long, bool> func)
        {
            if (!func(arg))
            {
                throw new ArgumentOutOfRangeException(nameof(arg) + " is out of range");
            }
        }
    }
}