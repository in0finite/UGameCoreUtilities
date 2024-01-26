using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public static class EnumeratorExtensions
    {
        /// <summary>
        /// Enumerates this <see cref="IEnumerator{T}"/> to the end, and returns last value retrieved from it.
        /// </summary>
        public static T EnumerateToEnd<T>(this IEnumerator<T> enumerator)
        {
            T result = default;
            while (enumerator.MoveNext())
                result = enumerator.Current;
            return result;
        }

        /// <summary>
        /// Enumerates this <see cref="IEnumerator{T}"/> to the end, and returns last value retrieved from it.
        /// </summary>
        public static object EnumerateToEnd(this IEnumerator enumerator)
        {
            object result = null;
            while (enumerator.MoveNext())
                result = enumerator.Current;
            return result;
        }

        public static IEnumerator Append(this IEnumerator enumerator, IEnumerator other)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            while (other.MoveNext())
                yield return other.Current;
        }

        public static IEnumerator AppendAction(this IEnumerator enumerator, Action action)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            action();
        }
    }
}
