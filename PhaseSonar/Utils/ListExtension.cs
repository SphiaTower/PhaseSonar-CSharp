using System.Collections.Generic;

namespace PhaseSonar.Utils
{
    /// <summary>
    ///     Extension Toolbox for IList{T}
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        ///     Return if the list is empty.
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return list.Count == 0;
        }

        /// <summary>
        ///     Return if the list is not empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool NotEmpty<T>(this IList<T> list)
        {
            return list.Count != 0;
        }
    }
}