using System;
using System.Collections.Generic;
using System.Linq;

namespace UGameCore.Utilities
{
    public static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T> action)
        {
            foreach (var element in enumerable)
            {
                action(element);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, System.Action<T, int> action)
        {
            int i = 0;
            foreach (var element in enumerable)
            {
                action(element, i);
                i++;
            }
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> enumerable, bool condition, System.Func<T, bool> predicate)
        {
            return condition ? enumerable.Where(predicate) : enumerable;
        }

        public static T SingleOr<T>(this IEnumerable<T> enumerable, T defaultValue)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            try
            {
                // use Single() because it has optimizations if IEnumerable is ICollection, etc ...
                return enumerable.Single();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> enumerable, System.Predicate<T> predicate)
        {
            int i = 0;
            foreach (var elem in enumerable)
            {
                if (predicate(elem))
                    return i;
                i++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
        {
            // TODO: this can allocate memory if T is value-type
            return enumerable.FindIndex(elem => elem.Equals(value));
        }

        public static bool AnyInList<T>(this IList<T> list, System.Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                    return true;
            }
            return false;
        }

        public static bool AddIfNotPresent<T>(this List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        public static void EnsureCount<T>(this List<T> list, int count)
        {
            if (list.Count < count)
            {
                int diff = count - list.Count;
                for (int i = 0; i < diff; i++)
                {
                    list.Add(default);
                }
            }
        }

        public static T RemoveLast<T>(this IList<T> list)
        {
            T lastElement = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return lastElement;
        }

        public static T RemoveFirst<T>(this IList<T> list)
        {
            T firstElement = list[0];
            list.RemoveAt(0);
            return firstElement;
        }

        public static Queue<T> ToQueue<T>(this IEnumerable<T> enumerable)
        {
            return new Queue<T>(enumerable);
        }

        public static Queue<T> ToQueueWithCapacity<T>(this IEnumerable<T> enumerable, int capacity)
        {
            var queue = new Queue<T>(capacity);
            foreach (var item in enumerable)
                queue.Enqueue(item);
            return queue;
        }

        public static T[] ToArrayOfLength<T>(this IEnumerable<T> enumerable, int length)
        {
            T[] array = new T[length];
            int i = 0;
            foreach (var item in enumerable)
            {
                array[i] = item;
                i++;
            }
            return array;
        }

        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, bool condition, T element)
        {
            return condition ? enumerable.Append(element) : enumerable;
        }

        public static void AddMultiple<T>(this ICollection<T> collection, T value, int count)
        {
            for (int i = 0; i < count; i++)
                collection.Add(value);
        }

        public static void AddMultiple<T>(this ICollection<T> collection, int count)
        {
            collection.AddMultiple(default, count);
        }

        public static IEnumerable<T> DistinctBy<T, T2>(this IEnumerable<T> enumerable, System.Func<T, T2> selector)
        {
            var hashSet = new HashSet<T2>();

            foreach (T elem in enumerable)
            {
                T2 value = selector(elem);
                if (hashSet.Add(value))
                    yield return elem;
            }
        }

        public static T MinBy<T, TComparable>(
            this IEnumerable<T> enumerable,
            System.Func<T, TComparable> selector,
            T elementToReturnIfEmpty)
            where TComparable : IComparable
        {
            var comparer = Comparer<TComparable>.Default;
            T minElement = default;
            TComparable minValue = default;
            bool hasAnyValue = false;

            foreach (var element in enumerable)
            {
                TComparable value = selector(element);
                if (!hasAnyValue)
                {
                    hasAnyValue = true;
                    minElement = element;
                    minValue = value;
                }
                else
                {
                    if (comparer.Compare(value, minValue) < 0)
                    {
                        minElement = element;
                        minValue = value;
                    }
                }
            }

            return !hasAnyValue ? elementToReturnIfEmpty : minElement;
        }

        public static T RandomElement<T>(this IList<T> list)
        {
            if (list.Count < 1)
                throw new System.InvalidOperationException("List has no elements");
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        private static T RandomElement<T>(this IEnumerable<T> enumerable, bool returnDefaultIfEmpty)
        {
            // ReSharper disable PossibleMultipleEnumeration
            int count = enumerable.Count();

            if (count < 1)
            {
                if (returnDefaultIfEmpty)
                    return default(T);
                throw new System.InvalidOperationException("Enumerable has no elements");
            }

            return enumerable.ElementAt(UnityEngine.Random.Range(0, count));
            // ReSharper restore PossibleMultipleEnumeration
        }

        public static T RandomElement<T>(this IEnumerable<T> enumerable)
        {
            return RandomElement(enumerable, false);
        }

        public static T RandomElementOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return RandomElement(enumerable, true);
        }
    }
}
