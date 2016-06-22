namespace PhaseSonar.Slicers
{
    /// <summary>
    ///     A slicer which slices adjacent pulses at the center of them.
    /// </summary>
    public class SymmetrySlicer : SimpleSlicer
    {
        /// <summary>
        ///     Create a slicer.
        /// </summary>
        /// <param name="crestFinder">
        ///     <see cref="ICrestFinder" />
        /// </param>
        public SymmetrySlicer(ICrestFinder crestFinder) : base(crestFinder)
        {
        }

        /// <summary>
        ///     The index of the crest in the slice.
        /// </summary>
        public override int CrestIndex => SlicedPeriodLength/2;
    }
}