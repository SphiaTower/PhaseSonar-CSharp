using System;
using System.Windows.Controls;
using JetBrains.Annotations;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class GeneralConfigurations {
        private static GeneralConfigurations _singleton;

        private GeneralConfigurations(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType, int queueSize, bool saveSample, bool saveSpec, bool saveAcc,
            OperationMode operationMode, int targetCnt) {
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
            OperationMode = operationMode;
            TargetCnt = targetCnt;
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
        public OperationMode OperationMode { get; set; }
        public int TargetCnt { get; set; }

        public static void Register(GeneralConfigurations generalConfigurations) {
            if (_singleton == null) {
                _singleton = generalConfigurations;
            } else {
                ConfigsHolder.CopyTo(generalConfigurations, _singleton);
            }
        }



        public static void Initialize(double repetitionRate, int threadNum, int dispPoints, string directory,
            bool viewPhase, SaveType saveType, int queueSize, bool saveSample, bool saveSpec, bool saveAcc,
            OperationMode operationMode, int targetCnt) {
            if (_singleton != null) {
                throw new Exception("environment already init");
            }
            _singleton = new GeneralConfigurations(repetitionRate, threadNum, dispPoints, directory, viewPhase, saveType,
                queueSize, saveSample, saveSpec, saveAcc, operationMode, targetCnt);
        }

        public static GeneralConfigurations Get() {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }
    }
}