using System;
using System.Windows.Controls;
using FTIR.Utils;

namespace Shokouki.Configs
{
    [Serializable]
    public class Configurations
    {
        private static Configurations _singleton;

        private Configurations(double repetitionRate, int threadNum, int dispPoints, string directory)
        {
            RepetitionRate = repetitionRate;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            Directory = directory;
        }

        public double RepetitionRate { get; set; }

        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }

        internal void Register(Configurations configurations) {
            _singleton = configurations;
        }

        public string Directory { get; set; }


        public void Bind(Control repetitionRate, Control threadNum, Control dispPoints, Control savePath)
        {
            repetitionRate.DataContext = this;
            threadNum.DataContext = this;
            dispPoints.DataContext = this;
            savePath.DataContext = this;
        }

        public static void Initialize(double repetitionRate, int threadNum, int dispPoints, string directory)
        {
            if (_singleton != null)
            {
                throw new Exception("environment already init");
            }
            _singleton = new Configurations(repetitionRate, threadNum, dispPoints, directory);
        }

        public static Configurations Get()
        {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}