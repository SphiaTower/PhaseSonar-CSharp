using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace PhaseSonar.Utils {
    public sealed class Maybe<T>
    {
        [CanBeNull]
        private readonly T _value;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        private Maybe([CanBeNull]T value)
        {
            _value = value;
        }

        public static Maybe<T> OfNullable([CanBeNull]T value)
        {
            return new Maybe<T>(value);
        }


        public static Maybe<T> Of([NotNull]T value) {
            return new Maybe<T>(value);
        }


        public static Maybe<T> Empty() {
            return EmptyInstance;
        }

        private static readonly Maybe<T> EmptyInstance = new Maybe<T>(default(T));

        public void IfPresent(Action<T> action)
        {
            if (IsPresent())
            {
                action.Invoke(_value);
            }
        }

        public bool IsPresent()
        {
            return _value != null;
        }
    }
}
