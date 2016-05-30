using System;
using System.Linq;
using FTIR.Correctors;
using FTIR.Utils;

namespace Shokouki.Controllers
{
    public class SpectrumCamera<T> : DiskCameraBase<T> where T : ISpectrum
    {
        public SpectrumCamera(string directory, string prefix, bool on) : base(directory, prefix, on)
        {
        }

        protected override bool OnConsume(T dequeue, string basePath, int index)
        {
            if (dequeue is RealSpectrum)
            {
                var spec = dequeue as RealSpectrum;
                Toolbox.WriteData(basePath +"t"+ TimeStamp+"-"+ index + "-n" + dequeue.PulseCount+ Suffix,
                    spec.AmplitudeArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                return true;
            }
            else if (dequeue is ComplexSpectrum)
            {
                var spec = dequeue as ComplexSpectrum;
                Toolbox.WriteData(basePath + "real-" + "t" + TimeStamp + "-" + index+ "n" + dequeue.PulseCount  + Suffix,
                    spec.RealArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                Toolbox.WriteData(basePath + "imag-" + "t" + TimeStamp + "-" + index + "n" + dequeue.PulseCount + Suffix,
                    spec.ImagArray.Select(d => d*d/dequeue.PulseCount/dequeue.PulseCount).ToArray());
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class SampleCamera : DiskCameraBase<double[]>
    {
        public SampleCamera(string directory, string prefix, bool on) : base(directory, prefix, on)
        {
        }

        protected override bool OnConsume(double[] dequeue, string basePath, int fileIndex)
        {
            try
            {
                Toolbox.SerializeData(basePath + TimeStamp + "-" + fileIndex + Suffix, dequeue);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }
    }
}