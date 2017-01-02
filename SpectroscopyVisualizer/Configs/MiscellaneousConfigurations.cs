using System;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class MiscellaneousConfigurations {
        private static MiscellaneousConfigurations _singleton;

        private MiscellaneousConfigurations(int waitEmptyProducerMsTimeout, int minFlatPhasePtsNumCnt,
            double maxPhaseStd, string pythonPath) {
            MinFlatPhasePtsNumCnt = minFlatPhasePtsNumCnt;
            MaxPhaseStd = maxPhaseStd;
            WaitEmptyProducerMsTimeout = waitEmptyProducerMsTimeout;
            PythonPath = pythonPath;
        }

        public int MinFlatPhasePtsNumCnt { get; set; } //200
        public double MaxPhaseStd { get; set; } //0.34

        public int WaitEmptyProducerMsTimeout { get; set; } //5000

        public string PythonPath { get; set; }

        public static MiscellaneousConfigurations Get() {
            Toolbox.RequireNonNull(_singleton, "environment not init");
            return _singleton;
        }

        public static void Initialize(int waitEmptyProducerMsTimeout, int minFlatPhasePtsNumCnt, double maxPhaseStd,
            string pythonPath) {
            _singleton = new MiscellaneousConfigurations(waitEmptyProducerMsTimeout, minFlatPhasePtsNumCnt, maxPhaseStd,
                pythonPath);
        }

        public static void Register(MiscellaneousConfigurations miscellaneous) {
            if (_singleton == null) {
                _singleton = miscellaneous;
            } else {
                ConfigsHolder.CopyTo(miscellaneous, _singleton);
            }
        }
    }
}