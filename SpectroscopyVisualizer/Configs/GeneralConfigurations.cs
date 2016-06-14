using System;
using System.Windows.Controls;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs
{
    [Serializable]
    public class GeneralConfigurations
    {
        private static GeneralConfigurations _singleton;

        private GeneralConfigurations(double repetitionRate, int threadNum, int dispPoints, string directory)
        {
            RepetitionRate = repetitionRate;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            Directory = directory;
        }

        public double RepetitionRate { get; set; }

        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }

        public string Directory { get; set; }

        internal void Register(GeneralConfigurations generalConfigurations)
        {
            _singleton = generalConfigurations;
        }


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
            _singleton = new GeneralConfigurations(repetitionRate, threadNum, dispPoints, directory);
        }

        public static GeneralConfigurations Get()
        {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}