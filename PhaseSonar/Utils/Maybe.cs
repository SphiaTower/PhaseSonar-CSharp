using System;
using JetBrains.Annotations;

namespace PhaseSonar.Utils
{
    /// <summary>
    ///     A maybe type that could contains value or null
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    public sealed class Maybe<T>
    {
        private static readonly Maybe<T> EmptyInstance = new Maybe<T>(default(T));

        [CanBeNull] private readonly T _value;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        private Maybe([CanBeNull] T value)
        {
            _value = value;
        }

        public static Maybe<T> OfNullable([CanBeNull] T value)
        {
            return new Maybe<T>(value);
        }


        public static Maybe<T> Of([NotNull] T value)
        {
            return new Maybe<T>(value);
        }


        public static Maybe<T> Empty()
        {
            return EmptyInstance;
        }

        public void IfPresent(Action<T> action)
        {
            if (IsPresent())
            {
                action.Invoke(_value);
            }
        }

        public void IfNotPresent(Action action)
        {
            if (!IsPresent())
            {
                action.Invoke();
            }
        }

        public bool IsPresent()
        {
            return _value != null;
        }
    }
}