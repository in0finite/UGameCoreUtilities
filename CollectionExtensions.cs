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

        public static void ForEachIndexed<T>(this IList<T> list, System.Action<T, int> action)
        {
            for (int i = 0; i < list.Count; i++)
                action(list[i], i);
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> enumerable, bool condition, System.Func<T, bool> predicate)
        {
            return condition ? enumerable.Where(predicate) : enumerable;
        }

        public static IEnumerable<T> WhereIf<T>(
            this IEnumerable<T> enumerable, bool condition, System.Func<T, int, bool> predicate)
        {
            return condition ? enumerable.Where(predicate) : enumerable;
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            return enumerable.Where(_ => _ != null);
        }

        public static T SingleOr<T>(this IEnumerable<T> enumerable, T defaultValue)
        {
            return enumerable.TryGetSingle(out T singleElem) ? singleElem : defaultValue;
        }

        public static bool TryGetSingle<T>(this IEnumerable<T> enumerable, out T result)
        {
            if (enumerable.TryGetCountFast(out int countFast) && countFast != 1)
            {
                result = default;
                return false;
            }

            T tempResult = default;
            long count = 0;
            foreach (T item in enumerable)
            {
                if (count == 1)
                {
                    result = default;
                    return false;
                }

                tempResult = item;
                count++;
            }

            result = tempResult;
            return true;
        }

        public static bool ContainsNonAlloc<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) >= 0;
        }

        public static bool TryFind<T>(this IEnumerable<T> enumerable, System.Predicate<T> predicate, out T result)
        {
            foreach (T elem in enumerable)
            {
                if (predicate(elem))
                {
                    result = elem;
                    return true;
                }
            }

            result = default;
            return false;
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

        public static bool TryFindIndex<T>(this IEnumerable<T> enumerable, System.Predicate<T> predicate, out int index)
        {
            index = enumerable.FindIndex(predicate);
            return index != -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T value)
        {
            var comparer = EqualityComparer<T>.Default;
            return enumerable.FindIndex(elem => comparer.Equals(value, elem));
        }

        public static int IndexOf<T>(this T[] arr, T value) => Array.IndexOf(arr, value);

        public static int ClampIndexWithinBounds<T>(this IReadOnlyCollection<T> collection, int index)
        {
            int count = collection.Count;
            if (count <= 0)
                return -1;

            if (index < 0)
                return 0;

            if (index >= count)
                return count - 1;

            return index;
        }

        public static int BinarySearchNextAndPrevious<T>(
            this List<T> list,
            T value,
            IComparer<T> comparer,
            out int indexPrevious,
            out int indexNext,
            bool nextIsCurrentIfMatching = true)
        {
            if (list.Count == 0) // need to check, to avoid some edge cases below
            {
                indexNext = -1;
                indexPrevious = -1;
                return -1;
            }

            int index = list.BinarySearch(value, comparer);

            if (index >= 0) // found matching value
            {
                if (nextIsCurrentIfMatching)
                {
                    indexNext = index;
                    indexPrevious = index - 1;
                }
                else
                {
                    indexNext = index + 1;
                    if (indexNext >= list.Count)
                        indexNext = -1;

                    indexPrevious = index;
                }
                
                return index;
            }

            int indexOfHigher = ~index;
            int listCountMinus1 = list.Count - 1;

            indexNext = indexOfHigher;
            if (!indexNext.BetweenInclusive(0, listCountMinus1))
                indexNext = -1;

            indexPrevious = indexOfHigher - 1;
            if (!indexPrevious.BetweenInclusive(0, listCountMinus1))
                indexPrevious = -1;

            return index;
        }

        public static int BinarySearchNextAndPrevious<T>(
            this List<T> list,
            T value,
            IComparer<T> comparer,
            out T? previousElement,
            out T? nextElement,
            bool nextIsCurrentIfMatching = true)
            where T : struct
        {
            int index = list.BinarySearchNextAndPrevious(
                value, comparer, out int previousElementIndex, out int nextElementIndex, nextIsCurrentIfMatching);

            previousElement = previousElementIndex < 0 ? null : list[previousElementIndex];
            nextElement = nextElementIndex < 0 ? null : list[nextElementIndex];
            
            return index;
        }

        public static bool TryGetCountFast<T>(this IEnumerable<T> enumerable, out int count)
        {
            if (enumerable is ICollection<T> collectionGeneric)
            {
                count = collectionGeneric.Count;
                return true;
            }

            if (enumerable is System.Collections.ICollection collection)
            {
                count = collection.Count;
                return true;
            }

            if (enumerable is IReadOnlyCollection<T> collectionGenericReadOnly)
            {
                count = collectionGenericReadOnly.Count;
                return true;
            }

            count = 0;
            return false;
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

        public static void EnsureCountAtLeast<T>(this List<T> list, int count) => list.EnsureCount(count);

        public static void EnsureCountExactly<T>(this List<T> list, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            if (list.Count <= count)
            {
                list.EnsureCount(count);
            }
            else
            {
                int numToRemove = list.Count - count;
                list.RemoveRange(count, numToRemove);
            }
        }

        public static T[] EnsureCount<T>(this T[] array, int count)
        {
            if (array.Length < count)
            {
                T[] newArray = new T[count];
                array.CopyTo(newArray, 0);
                return newArray;
            }

            return array;
        }

        public static void EnsureCapacity<T>(this List<T> list, int capacity)
        {
            if (list.Capacity < capacity)
                list.Capacity = capacity;
        }

        /// <summary>
        /// Initializes every element of the list by calling the default constructor.
        /// </summary>
        public static void ConstructAll<T>(this IList<T> list)
            where T : new()
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = new T();
        }

        /// <summary>
        /// Replaces each element in the list with the value returned from selector.
        /// </summary>
        public static void ReplaceEach<T>(this IList<T> list, Func<T, T> selector)
        {
            for (int i = 0; i < list.Count; i++)
                list[i] = selector(list[i]);
        }

        public static void TrimExcessSmart<T>(
            this List<T> list,
            float sizeUpperLimitRatio = 1f / 3f,
            float newCapacityMultiplier = 1.5f,
            int minCapacityToTrim = 32)
        {
            if (list.Capacity <= minCapacityToTrim)
                return;

            int sizeUpperLimit = (int)Math.Ceiling(list.Capacity * sizeUpperLimitRatio);
            if (list.Count >= sizeUpperLimit)
                return;

            int newCapacity = (int)(list.Count * newCapacityMultiplier);
            newCapacity = Math.Max(newCapacity, minCapacityToTrim);

            list.Capacity = newCapacity;
        }

        public static T RemoveLast<T>(this List<T> list)
        {
            int lastIndex = list.Count - 1;
            T lastElement = list[lastIndex];
            list.RemoveAt(lastIndex);
            return lastElement;
        }

        public static T RemoveFirst<T>(this List<T> list)
        {
            T firstElement = list[0];
            list.RemoveAt(0);
            return firstElement;
        }

        public static void RemoveRangeFromEnd<T>(this List<T> list, int index)
        {
            int numToRemove = list.Count - index;
            list.RemoveRange(index, numToRemove);
        }

        public static int RemoveAll<T>(this List<T> list, System.Func<T, int, bool> predicate)
        {
            int i = 0;
            return list.RemoveAll(_ =>
            {
                bool b = predicate(_, i);
                i++;
                return b;
            });
        }

        public static int RemoveAllNullReferences<T>(this List<T> list)
            where T : class
        {
            return list.RemoveAll(static item => object.ReferenceEquals(item, null));
        }

        public static void Clear(this System.Array array)
        {
            System.Array.Clear(array, 0, array.Length);
        }

        public static void Clear(this System.Array array, int index, int length)
        {
            System.Array.Clear(array, index, length);
        }

        public static int EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> range)
        {
            int numAdded = 0;
            foreach (T item in range)
            {
                queue.Enqueue(item);
                numAdded++;
            }
            return numAdded;
        }

        public static void DequeueMultiple<T>(this Queue<T> queue, int num)
        {
            for (int i = 0; i < num; i++)
                queue.Dequeue();
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

        public static T[] ToArray<T>(this List<T> list, int index, int length)
        {
            T[] array = new T[length];
            list.CopyTo(index, array, 0, length);
            return array;
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

        public static T[] ToArrayOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            T[] array = enumerable.ToArray();
            if (array.Length == 0)
                return Array.Empty<T>();
            return array;
        }

        public static T[] ToArrayOrEmpty<T>(this ICollection<T> collection)
        {
            int count = collection.Count;
            if (count == 0)
                return Array.Empty<T>();
            T[] array = new T[count];
            collection.CopyTo(array, 0);
            return array;
        }

        public static bool IsNullOrEmpty<TCollection>(this TCollection collection)
            where TCollection : System.Collections.ICollection
        {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
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

        public static TNew[] ConvertArray<TNew, TOld>(this TOld[] array, Func<TOld, TNew> selector)
        {
            int length = array.Length;
            TNew[] newArray = new TNew[length];
            for (int i = 0; i < length; i++)
            {
                newArray[i] = selector(array[i]);
            }
            return newArray;
        }

        public static bool HasDuplicates<T>(this IEnumerable<T> enumerable)
        {
            var hashSet = new HashSet<T>();

            foreach (T elem in enumerable)
            {
                if (!hashSet.Add(elem))
                    return true;
            }

            return false;
        }

        public static IEnumerable<T> DistinctBy<T, T2>(
            this IEnumerable<T> enumerable, System.Func<T, T2> selector, IEqualityComparer<T2> equalityComparer)
        {
            var hashSet = new HashSet<T2>(equalityComparer);

            foreach (T elem in enumerable)
            {
                T2 value = selector(elem);
                if (hashSet.Add(value))
                    yield return elem;
            }
        }

        public static IEnumerable<T> DistinctBy<T, T2>(this IEnumerable<T> enumerable, System.Func<T, T2> selector)
        {
            return enumerable.DistinctBy(selector, EqualityComparer<T2>.Default);
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

        public static void SortBy<T, TBy>(this List<T> list, Func<T, TBy> funcSelector)
        {
            list.SortBy(funcSelector, Comparer<TBy>.Default);
        }

        public static void SortBy<T, TBy>(
            this List<T> list, Func<T, TBy> funcSelector, IComparer<TBy> comparer)
        {
            list.Sort((a, b) => comparer.Compare(funcSelector(a), funcSelector(b)));
        }

        public static void Sort<T>(this T[] array)
        {
            Array.Sort(array);
        }

        public static void Sort<T>(this T[] array, IComparer<T> comparer)
        {
            Array.Sort(array, comparer);
        }

        public static void Sort<T>(this T[] array, Comparison<T> comparison)
        {
            Array.Sort(array, comparison);
        }

        public static void SortBy<T, TBy>(this T[] array, Func<T, TBy> funcSelector)
            where TBy : IComparable<TBy>
        {
            Array.Sort(array, (a, b) => funcSelector(a).CompareTo(funcSelector(b)));
        }

        public static void SortBy<T, TBy>(this T[] array, Func<T, TBy> funcSelector, IComparer<TBy> comparer)
        {
            Array.Sort(array, (a, b) => comparer.Compare(funcSelector(a), funcSelector(b)));
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

        public static int RemoveAll<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            if (dictionary.Count == 0) // early exit, no memory allocation below for IEnumerator
                return 0;

            List<TKey> keysToRemove = null;

            foreach (var pair in dictionary)
            {
                if (predicate(pair))
                {
                    keysToRemove ??= new List<TKey>();
                    keysToRemove.Add(pair.Key);
                }
            }

            if (keysToRemove == null)
                return 0;

            int numRemoved = 0;
            foreach (TKey key in keysToRemove)
            {
                if (dictionary.Remove(key))
                    numRemoved++;
            }

            return numRemoved;
        }

        public static void Replace<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                throw new InvalidOperationException($"Specified key {key} not found in Dictionary");

            dictionary[key] = value;
        }

        public static void ClearAndTrimExcess<T>(this HashSet<T> hashSet, int countToTrimExcess)
        {
            int count = hashSet.Count;
            hashSet.Clear();
            if (count > countToTrimExcess)
                hashSet.TrimExcess();
        }

        public static LinkedListNode<T> InsertSorted<T>(
            this LinkedList<T> linkedList, T valueToAdd, Comparison<T> comparison)
        {
            var nodeToAdd = new LinkedListNode<T>(valueToAdd);

            if (linkedList.Count == 0)
            {
                linkedList.AddFirst(nodeToAdd);
                return nodeToAdd;
            }

            var node = linkedList.First;
            while (node != null)
            {
                if (comparison(valueToAdd, node.Value) < 0)
                {
                    linkedList.AddBefore(node, nodeToAdd);
                    return nodeToAdd;
                }

                node = node.Next;
            }

            linkedList.AddLast(nodeToAdd);
            return nodeToAdd;
        }
    }
}
