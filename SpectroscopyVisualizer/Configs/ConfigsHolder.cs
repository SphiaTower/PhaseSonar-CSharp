using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PhaseSonar.Utils;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SpectroscopyVisualizer.Configs {
    [Serializable]
    public class ConfigsHolder {
        private readonly CorrectorConfigurations _corrector = CorrectorConfigurations.Get();
        private readonly GeneralConfigurations _general = GeneralConfigurations.Get();
        private readonly SamplingConfigurations _sampling = SamplingConfigurations.Get();
        private readonly SliceConfigurations _slice = SliceConfigurations.Get();
        public static void CopyTo<T>(T source,T destiny) {
            var properties = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>();

            foreach (var property in properties) {
                property.SetValue(destiny, property.GetValue(source));
            }
        }
        public static void Load() {
            var dialog = new OpenFileDialog {
                Filter = "SpectroscopyVisualizer Config Files (*.svcfg)|*.svcfg|Show All Files (*.*)|*.*",
                Title = "Load Configs"
            };
            if (dialog.ShowDialog() == DialogResult.OK) {
                var configsHolder = Toolbox.DeserializeData<ConfigsHolder>(dialog.FileName);
                configsHolder.Register();
            }
        }
        public static void Load(string path) {
                var configsHolder = Toolbox.DeserializeData<ConfigsHolder>(path);
                configsHolder.Register();
        }

        public void Register() {
            CorrectorConfigurations.Register(_corrector);
            GeneralConfigurations.Register(_general);
            SamplingConfigurations.Register(_sampling);
            SliceConfigurations.Register(_slice);
        }

        public void Dump() {
            var dialog = new SaveFileDialog {
                Filter = "SpectroscopyVisualizer Config Files (*.svcfg)|*.svcfg|Show All Files (*.*)|*.*",
                FileName = "Configs"
            };
            if (dialog.ShowDialog() == true) {
                Toolbox.SerializeData(dialog.FileName, this);
            }
        }

        public void Dump([NotNull] string path) {
           Toolbox.SerializeData(path, this);
        }
    }
}