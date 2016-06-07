using System.Linq;
using PhaseSonar.Correctors;
using PhaseSonar.Utils;

namespace SpectroscopyVisualizer.Writers
{
    public class SpectrumWriter<T> : DiskWriterBase<T> where T : ISpectrum
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="prefix">The prefix of the file.</param>
        /// <param name="on">On or off.</param>
        public SpectrumWriter(string directory, string prefix, bool on) : base(directory, prefix, on)
        {
        }

        /// <summary>
        /// Called when start consuming a dequeued element.
        /// </summary>
        /// <param name="dequeue"></param>
        /// <param name="basePath"></param>
        /// <param name="fileIndex"></param>
        /// <returns>True if consumed successfully.</returns>
        protected override bool WriteData(T dequeue, string basePath, int fileIndex)
        {
            if (dequeue is RealSpectrum)
            {
                var spec = dequeue as RealSpectrum;
                Toolbox.WriteData(basePath +"t"+ TimeStamp+"-"+ fileIndex + "-n" + dequeue.PulseCount+ Suffix,
                    spec.AmplitudeArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                return true;
            }
            else if (dequeue is ComplexSpectrum)
            {
                var spec = dequeue as ComplexSpectrum;
                Toolbox.WriteData(basePath + "real-" + "t" + TimeStamp + "-" + fileIndex+ "n" + dequeue.PulseCount  + Suffix,
                    spec.RealArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                Toolbox.WriteData(basePath + "imag-" + "t" + TimeStamp + "-" + fileIndex + "n" + dequeue.PulseCount + Suffix,
                    spec.ImagArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}