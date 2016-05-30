using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class ConfigsHolder
    {
        public Configurations Configurations { get; set; } = Configurations.Get();
        public CorrectorConfigs CorrectorConfigs { get; set; } = CorrectorConfigs.Get();
        public SliceConfigs SliceConfigs { get; set; } = SliceConfigs.Get();
        public SamplingConfigs SamplingConfigs { get; set; } = SamplingConfigs.Get();

        public void Register()
        {
            Configurations.Register(Configurations);
            CorrectorConfigs.Register(CorrectorConfigs);
            SliceConfigs.Register(SliceConfigs);
            SamplingConfigs.Register(SamplingConfigs);
        }
    }
}
