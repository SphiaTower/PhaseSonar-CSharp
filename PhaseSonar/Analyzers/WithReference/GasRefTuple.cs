using PhaseSonar.Correctors;

namespace PhaseSonar.Analyzers.WithReference {
    /// <summary>
    ///     A container for gas and reference spectra
    /// </summary>
    public class GasRefTuple {
        private GasRefTuple(ISpectrum gas, ISpectrum reference) {
            Gas = gas;
            Reference = reference;
        }

        /// <summary>
        ///     The spectrum of the gas
        /// </summary>
        public ISpectrum Gas { get; }

        /// <summary>
        ///     The spectrum of the reference
        /// </summary>
        public ISpectrum Reference { get; }

        /// <summary>
        ///     Create an instance with source and reference known
        /// </summary>
        /// <param name="source">The source spectrum</param>
        /// <param name="reference">The reference spectrum</param>
        /// <returns></returns>
        public static GasRefTuple SourceAndRef(ISpectrum source, ISpectrum reference) {
            return new GasRefTuple(source, reference);
        }

        /// <summary>
        ///     Create an result instace without the knowledge of which component is gas and which is reference
        /// </summary>
        /// <param name="either">A spectrum</param>
        /// <param name="other">Another spectrum</param>
        /// <returns></returns>
        public static GasRefTuple EitherAndOther(ISpectrum either, ISpectrum other) {
            double eitherSum = 0, otherSum = 0;
            for (var i = 0; i < either.Length(); i++) {
                eitherSum += either.Intensity(i);
                otherSum += other.Intensity(i);
            }
            return eitherSum >= otherSum
                ? SourceAndRef(either, other)
                : SourceAndRef(other, either);
        }
    }
}