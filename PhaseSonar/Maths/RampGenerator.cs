using System;
using System.Collections.Generic;

namespace PhaseSonar.Maths {
    /// <summary>
    ///     A static generator for ramp functions, which caches the result.
    /// </summary>
    public class RampGenerator {
        private static readonly object Lock = new object();
        private static volatile Dictionary<Tuple<int, int>, double[]> _rampCache;

        private static Dictionary<Tuple<int, int>, double[]> RampCache {
            get {
                if (_rampCache == null) {
                    lock (Lock) {
                        if (_rampCache == null) {
                            _rampCache = new Dictionary<Tuple<int, int>, double[]>();
                        }
                    }
                }
                return _rampCache;
            }
        }

        /// <summary>
        ///     Generate a triangular array, the value of which is 1 at the crest, and 0 at head and tail.
        ///     The result is cached globally.
        /// </summary>
        /// <param name="length">The length of the output array</param>
        /// <param name="crestIndex">The index of the crest of array</param>
        /// <returns>The triangular array</returns>
        public static double[] Ramp(int length, int crestIndex) {
            var key = new Tuple<int, int>(length, crestIndex);
            try {
                return RampCache[key];
            } catch (KeyNotFoundException) {
                var rampArray = new double[length];
                var firstHalfInterval = 1.0/crestIndex;
                var lastHalfInterval = 1.0/(length - crestIndex - 1);
                for (var i = 0; i <= crestIndex; i++) {
                    rampArray[i] = firstHalfInterval*i;
                }
                for (var i = crestIndex + 1; i < length; i++) {
                    rampArray[i] = 1 - lastHalfInterval*(i - crestIndex);
                }
                RampCache[key] = rampArray;
                return rampArray;
            }
        }
    }
}