using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Configs {
    /// <summary>
    /// A configuration holder which holds all configuration singletons.
    /// This holder is used to serialize and deserialize the configurations.
    /// </summary>
    [Serializable]
    public class ConfigsHolder
    {
        /// <summary>
        /// <see cref="GeneralConfigurations"/>
        /// </summary>
        public GeneralConfigurations GeneralConfigs { get; set; } = GeneralConfigurations.Get();
        /// <summary>
        /// <see cref="CorrectorConfigs"/>
        /// </summary>
        public CorrectorConfigurations CorrectorConfigs { get; set; } = CorrectorConfigurations.Get();
        /// <summary>
        /// <see cref="SliceConfigurations"/>
        /// </summary>
        public SliceConfigurations SliceConfigs { get; set; } = SliceConfigurations.Get();
        /// <summary>
        /// <see cref="SamplingConfigs"/>
        /// </summary>
        public SamplingConfigurations SamplingConfigs { get; set; } = SamplingConfigurations.Get();

        /// <summary>
        /// Register configuration instances as singletons.
        /// </summary>
        public void Register()
        {
            GeneralConfigs.Register(GeneralConfigs);
            CorrectorConfigurations.Register(CorrectorConfigs);
            SliceConfigs.Register(SliceConfigs);
            SamplingConfigs.Register(SamplingConfigs);
        }
    }
}
