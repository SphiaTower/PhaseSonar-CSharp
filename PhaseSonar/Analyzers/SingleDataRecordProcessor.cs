using PhaseSonar.Slicers;

namespace PhaseSonar.Analyzers {
    /// <summary>
    ///     An basic analyser which slices and processes the pulse sequence.
    /// </summary>
    public abstract class SingleDataRecordProcessor {
        /// <summary>
        ///     Create an SingleDataRecordProcessor
        /// </summary>
        /// <param name="slicer">
        ///     <see cref="ISlicer" />
        /// </param>
        protected SingleDataRecordProcessor(ISlicer slicer) {
            Slicer = slicer;
        }

        /// <summary>
        ///     <see cref="ISlicer" />
        /// </summary>
        public ISlicer Slicer { get; set; }
    }
}