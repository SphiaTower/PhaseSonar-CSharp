using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class GeneralConfigurations {
        private static GeneralConfigurations _singleton;

        private GeneralConfigurations(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType) {
            RepetitionRate = repetitionRate;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            Directory = directory;
            ViewPhase = viewPhase;
            SaveType = saveType;
        }

        public bool ViewPhase { get; set; }

        public double RepetitionRate { get; set; }

        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }

        public string Directory { get; set; }

        public SaveType SaveType { get; set; }

        public static void Register(GeneralConfigurations generalConfigurations) {
            if (_singleton == null) {
                _singleton = generalConfigurations;
            } else {
                ConfigsHolder.CopyTo(generalConfigurations, _singleton);
            }
        }


        public void Bind([NotNull] Control repetitionRate, [NotNull] Control threadNum, [NotNull] Control dispPoints,
            [NotNull] Control savePath, [NotNull] Control viewPhase, [NotNull] Control saveType) {
            repetitionRate.DataContext = this;
            threadNum.DataContext = this;
            dispPoints.DataContext = this;
            savePath.DataContext = this;
            viewPhase.DataContext = this;
            saveType.DataContext = this;
        }

        public static void Initialize(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new GeneralConfigurations(repetitionRate, threadNum, dispPoints, directory, viewPhase, saveType);
        }

        public static GeneralConfigurations Get() {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}