using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class GeneralConfigurations {
        private static GeneralConfigurations _singleton;

        private GeneralConfigurations(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType, int queueSize,bool saveSample, bool saveSpec, bool saveAcc) {
            RepetitionRate = repetitionRate;
            ThreadNum = threadNum;
            DispPoints = dispPoints;
            Directory = directory;
            ViewPhase = viewPhase;
            SaveType = saveType;
            QueueSize = queueSize;
            SaveSample = saveSample;
            SaveSpec = saveSpec;
            SaveAcc = saveAcc;
        }

        public bool ViewPhase { get; set; }

        public double RepetitionRate { get; set; }

        public int ThreadNum { get; set; }
        public int DispPoints { get; set; }
        public int QueueSize { get; set; }
        public bool SaveSample { get; set; }
        public bool SaveSpec { get; set; }
        public bool SaveAcc { get; set; }

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
            [NotNull] Control savePath, [NotNull] Control viewPhase, [NotNull] Control saveType,
            [NotNull] Control queueSize,Control saveSample,Control saveSpec, Control saveAcc) {
            repetitionRate.DataContext = this;
            threadNum.DataContext = this;
            dispPoints.DataContext = this;
            savePath.DataContext = this;
            viewPhase.DataContext = this;
            saveType.DataContext = this;
            queueSize.DataContext = this;
            saveSample.DataContext = this;
            saveSpec.DataContext = this;
            saveAcc.DataContext = this;
        }

        public static void Initialize(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType, int queueSize, bool saveSample, bool saveSpec, bool saveAcc) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new GeneralConfigurations(repetitionRate, threadNum, dispPoints, directory, viewPhase, saveType,
                queueSize,  saveSample,  saveSpec,  saveAcc);
        }

        public static GeneralConfigurations Get() {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}