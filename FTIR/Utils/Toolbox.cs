using System;
using System.Collections;
using System.IO;

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

        public static void WriteData<T>(string path, T[] data)
        {
            var contents = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                contents[i] = data[i].ToString();
            }
            File.WriteAllLines(path, contents);
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