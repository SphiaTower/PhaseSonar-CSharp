using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace PhaseSonar.Utils {
    public static class Duo {
        [NotNull]
        public static Duo<T> Create<T>(T item1, T item2) {
            return new Duo<T>(item1, item2);
        }

        [NotNull]
        public static Duo<T> ToDuo<T>([NotNull] this IEnumerable<T> enumerable) {
            var enumerator = enumerable.GetEnumerator();
            enumerator.MoveNext();
            var item1 = enumerator.Current;
            enumerator.MoveNext();
            var item2 = enumerator.Current;
            return new Duo<T>(item1, item2);
        }
    }

    public class Duo<T> : IEnumerable<T> {
        private readonly IList<T> _listImplementation;


        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and
        ///     has the default initial capacity.
        /// </summary>
        public Duo(T item1, T item2) {
            Item1 = item1;
            Item2 = item2;
            _listImplementation = new List<T> {item1, item2};
        }

        public T Item1 { get; set; }
        public T Item2 { get; set; }

        /// <summary>Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
        public int Count {
            get { return _listImplementation.Count; }
        }

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        ///     The property is set and the
        ///     <see cref="T:System.Collections.Generic.IList`1" /> is read-only.
        /// </exception>
        public T this[int index] {
            get { return _listImplementation[index]; }
            set { _listImplementation[index] = value; }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator() {
            return _listImplementation.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) _listImplementation).GetEnumerator();
        }


        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.</summary>
        /// <returns>
        ///     true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />;
        ///     otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public bool Contains(T item) {
            return _listImplementation.Contains(item);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
        ///     <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
        ///     from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have
        ///     zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="array" /> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="arrayIndex" /> is less than 0.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The number of elements in the source
        ///     <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from
        ///     <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex) {
            _listImplementation.CopyTo(array, arrayIndex);
        }


        /// <summary>Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.</summary>
        /// <returns>The index of <paramref name="item" /> if found in the list; otherwise, -1.</returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public int IndexOf(T item) {
            return _listImplementation.IndexOf(item);
        }
    }
}