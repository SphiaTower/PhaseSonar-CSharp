namespace SpectroscopyVisualizer.Producers
{
    /// <summary>
    ///     A wrapper class for pulse sequences, providing id.
    /// </summary>
    public class SampleRecord
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SampleRecord(double[] pulseSequence, int id)
        {
            PulseSequence = pulseSequence;
            Id = id;
        }

        /// <summary>
        ///     The data it wraps.
        /// </summary>
        public double[] PulseSequence { get; }

        /// <summary>
        ///     The id.
        /// </summary>
        public int Id { get; }
    }
}