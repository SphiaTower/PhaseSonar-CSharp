using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class GeneralConfigurations {
        private static GeneralConfigurations _singleton;

        public bool ViewPhase { get; set; }

        private GeneralConfigurations(double repetitionRate, int threadNum, int dispPoints, string directory,bool viewPhase) {
            RepetitionRate = repetitionRate;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            Directory = directory;
            ViewPhase = viewPhase;
        }

        public double RepetitionRate { get; set; }

        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }

        public string Directory { get; set; }

        internal void Register(GeneralConfigurations generalConfigurations) {
            _singleton = generalConfigurations;
        }


        public void Bind([NotNull] Control repetitionRate, [NotNull] Control threadNum, [NotNull] Control dispPoints,
            [NotNull] Control savePath, [NotNull] Control viewPhase) {
            repetitionRate.DataContext = this;
            threadNum.DataContext = this;
            dispPoints.DataContext = this;
            savePath.DataContext = this;
            viewPhase.DataContext = this;
        }

        public static void Initialize(double repetitionRate, int threadNum, int dispPoints, string directory,bool viewPhase) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new GeneralConfigurations(repetitionRate, threadNum, dispPoints, directory,viewPhase);
        }

        public static GeneralConfigurations Get() {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}