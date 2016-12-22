using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using PhaseSonar.Correctors;
using PhaseSonar.Maths;
using PhaseSonar.Utils;
using SpectroscopyVisualizer.Configs;
using SpectroscopyVisualizer.Consumers;

namespace SpectroscopyVisualizer.Writers {
    public class SpectrumWriterV2 : IWriterV2<TracedSpectrum> {
        protected const string Suffix = ".txt";
        private readonly IWriterV2<TracedSpectrum> _writerV2Implementation;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SpectrumWriterV2([NotNull] string directory, [NotNull] string prefix, SaveType saveType) {
            _writerV2Implementation = new SerialWriterV2<TracedSpectrum>();
            var dateTime = DateTime.Now;
            var timeStamp = dateTime.ToString("HHmmssfff");
            var timeStr = timeStamp.Enclose("TS");
            var path = Path.Combine(directory, timeStr + prefix);

            if (saveType == SaveType.UnwrappedPhase) {
                _writerV2Implementation.ElementDequeued += spectrum => {
                    var phase = new double[spectrum.Length()];
                    for (var i = 0; i < spectrum.Length(); i++) {
                        phase[i] = spectrum.Array[i].Phase;
                    }
                    var unwrap = Functions.Unwrap(phase);
                    Toolbox.WriteStringArray(path + saveType.ToString().Enclose() + spectrum.Tag.Enclose("No") +
                                             spectrum.PulseCount.Enclose("Cnt") + Suffix,
                        unwrap.Select(p => p.ToString()).ToArray());
                };
            } else {
                var toStringFunc = GetToStringFunc(saveType);
                _writerV2Implementation.ElementDequeued += spectrum => {
                    Toolbox.WriteStringArray(
                        path + saveType.ToString().Enclose() + spectrum.Tag.Enclose() +
                        spectrum.PulseCount.Enclose("Cnt") + Suffix,
                        spectrum.ToStringArray(toStringFunc));
                };
            }
        }

        /// <summary>
        ///     Enqueue the data to save.
        /// </summary>
        /// <param name="data"></param>
        public void Write(TracedSpectrum data) {
            _writerV2Implementation.Write(data);
        }

        public event Action<TracedSpectrum> ElementDequeued {
            add { _writerV2Implementation.ElementDequeued += value; }
            remove { _writerV2Implementation.ElementDequeued -= value; }
        }

        [NotNull]
        private static Func<ISpectrum, int, string> GetToStringFunc(SaveType saveType) {
            switch (saveType) {
                case SaveType.Complex:
                    return SpectrumExtension.ToStringComplex;
                case SaveType.Magnitude:
                    return SpectrumExtension.ToStringMagnitude;
                case SaveType.Intensity:
                    return SpectrumExtension.ToStringIntensity;
                case SaveType.Phase:
                    return SpectrumExtension.ToStringPhase;
                case SaveType.Real:
                    return SpectrumExtension.ToStringReal;
                case SaveType.Imaginary:
                    return SpectrumExtension.ToStringImag;
                case SaveType.UnwrappedPhase:
                default:
                    throw new ArgumentOutOfRangeException(nameof(saveType), saveType, null);
            }
        }
    }
}