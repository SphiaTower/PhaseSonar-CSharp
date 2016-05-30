using System;
using System.Collections.Generic;

namespace PhaseSonar.Maths
{
    public class RampGenerator
    {
        private static readonly object Lock = new object();
        private static volatile Dictionary<Tuple<int, int>, double[]> _rampCache;

        private static Dictionary<Tuple<int, int>, double[]> RampCache
        {
            get
            {
                if (_rampCache == null)
                {
                    lock (Lock)
                    {
                        if (_rampCache == null)
                        {
                            _rampCache = new Dictionary<Tuple<int, int>, double[]>();
                        }
                    }
                }
                return _rampCache;
            }
        }

        public static double[] Ramp(int length, int peakIndex)
        {
            var key = new Tuple<int, int>(length, peakIndex);
            try
            {
                return RampCache[key];
            }
            catch (KeyNotFoundException)
            {
                var rampArray = new double[length];
                var firstHalfInterval = 1.0/peakIndex;
                var lastHalfInterval = 1.0/(length - peakIndex - 1);
                for (var i = 0; i <= peakIndex; i++)
                {
                    rampArray[i] = firstHalfInterval*i;
                }
                for (var i = peakIndex + 1; i < length; i++)
                {
                    rampArray[i] = 1 - lastHalfInterval*(i - peakIndex);
                }
                RampCache[key] = rampArray;
                return rampArray;
            }
        }
    }
}