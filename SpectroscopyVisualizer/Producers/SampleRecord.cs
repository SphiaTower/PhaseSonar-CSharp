using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectroscopyVisualizer.Producers {
    public class SampleRecord {
        public double[] PulseSequence { get; }
        public int ID { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public SampleRecord(double[] pulseSequence, int id)
        {
            PulseSequence = pulseSequence;
            ID = id;
        }
    }
}
