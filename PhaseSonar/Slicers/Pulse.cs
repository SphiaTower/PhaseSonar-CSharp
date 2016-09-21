using JetBrains.Annotations;

namespace PhaseSonar.Slicers {
    public interface IPulse {
        int CrestIndex { get; }

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection. </returns>
        int Count { get; }

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <returns>The element at the specified index in the read-only list.</returns>
        /// <param name="index">The zero-based index of the element to get. </param>
        double this[int index] { get; }

        double Sum();
    }

    public class Pulse : IPulse {
        [NotNull] private readonly double[] _pulseSequence;

        private readonly int _startIndexInSequence;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Pulse(double[] pulseSequence, int startIndexInSequence, int crestIndex, int count) {
            _pulseSequence = pulseSequence;
            _startIndexInSequence = startIndexInSequence;
            CrestIndex = crestIndex;
            Count = count;
        }

        public int CrestIndex { get; }

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection. </returns>
        public int Count { get; }

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <returns>The element at the specified index in the read-only list.</returns>
        /// <param name="index">The zero-based index of the element to get. </param>
        public double this[int index] => _pulseSequence[_startIndexInSequence + index];

        public double Sum() {
            var sum = .0;
            for (var i = _startIndexInSequence; i < _startIndexInSequence + Count; i++) {
                sum += _pulseSequence[i];
            }
            return sum;
        }
    }
}