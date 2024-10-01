using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class DictionaryAndList<TKey, TValue> : IEnumerable //, ICollection<KeyValuePair<TKey, TValue>>
    {
        struct DictEntry
        {
            public TValue Value;
            public ulong Counter;

            public DictEntry(TValue value, ulong counter)
            {
                Value = value;
                Counter = counter;
            }
        }

        public struct ListEntry
        {
            public TKey Key;
            public TValue Value;
            public ulong Counter;

            public ListEntry(TKey key, TValue value, ulong counter)
            {
                Key = key;
                Value = value;
                Counter = counter;
            }
        }

        public struct KeyValue
        {
            public TKey Key;
            public TValue Value;

            public KeyValue(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        readonly Dictionary<TKey, DictEntry> Dict;
        readonly List<ListEntry> List;

        readonly Predicate<ListEntry> ConsolidateListPredicate;
        readonly Predicate<ListEntry> RemoveAllPredicate;
        Predicate<KeyValue> UserRemoveAllPredicate;

        ulong LastElementCounter = 0;

        public int Count => Dict.Count;
        public int ListCount => List.Count;

        public bool IsReadOnly => false;


        public DictionaryAndList(int capacity, IEqualityComparer<TKey> comparer)
        {
            Dict = new(capacity, comparer);
            List = new(capacity);
            ConsolidateListPredicate = ShouldRemoveListElement;
            RemoveAllPredicate = RemoveAllPredicateFunction;
        }

        public DictionaryAndList(int capacity)
            : this(capacity, EqualityComparer<TKey>.Default)
        {
        }

        public DictionaryAndList()
            : this(16)
        {
        }

        public bool TryAdd(TKey key, TValue value)
        {
            ulong id = LastElementCounter + 1;

            if (!Dict.TryAdd(key, new DictEntry(value, id)))
                return false;

            LastElementCounter++;

            List.Add(new ListEntry(key, value, id));

            return true;
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new InvalidOperationException($"Item with the same key already exists: {key}");
        }

        public bool Remove(TKey key)
        {
            return Dict.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return Dict.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool bGet = Dict.TryGetValue(key, out var data);
            value = bGet ? data.Value : default;
            return bGet;
        }

        public void Clear()
        {
            Dict.Clear();
            List.Clear();
        }

        /// <summary>
        /// Note: this function is not re-entrable.
        /// </summary>
        public void RemoveAll(Predicate<KeyValue> predicate)
        {
            if (null == predicate)
                throw new ArgumentNullException(nameof(predicate));

            UserRemoveAllPredicate = predicate;
            List.RemoveAll(RemoveAllPredicate);
            UserRemoveAllPredicate = null;
        }

        bool RemoveAllPredicateFunction(ListEntry listEntry)
        {
            if (ShouldRemoveListElement(listEntry))
                return true;

            bool bRemove = UserRemoveAllPredicate(new KeyValue(listEntry.Key, listEntry.Value));

            if (bRemove)
                Dict.Remove(listEntry.Key);

            return bRemove;
        }

        bool ShouldRemoveListElement(ListEntry listEntry)
        {
            if (!Dict.TryGetValue(listEntry.Key, out DictEntry dictValue))
                return true;

            if (dictValue.Counter != listEntry.Counter)
                return true;

            return false;
        }

        /// <summary>
        /// Remove all entries from the List, which are not contained in the Dictionary.
        /// </summary>
        public void ConsolidateList()
        {
            // optimization if Dictionary is empty
            if (Dict.Count == 0)
            {
                List.Clear();
                return;
            }

            List.RemoveAll(ConsolidateListPredicate);

            if (List.Count != Dict.Count)
            {
                throw new ShouldNotHappenException($"List count ({List.Count}) should be equal to Dictionary count ({Dict.Count}) after consolidating. " +
                    $"There could be a bug in {nameof(IEqualityComparer<TKey>)} or {nameof(GetHashCode)} function");
            }
        }

        public bool GetAtIndex(int index, out KeyValue pair)
        {
            ListEntry listEntry = List[index];

            if (ShouldRemoveListElement(listEntry))
            {
                pair = default;
                return false;
            }

            pair = new KeyValue(listEntry.Key, listEntry.Value);
            return true;
        }

        public bool MoveNext(ref int index, out KeyValue pair)
        {
            for (; index < ListCount; index++)
            {
                if (GetAtIndex(index, out pair))
                    return true;
            }

            pair = default;
            return false;
        }

        public bool MovePrevious(ref int index, out KeyValue pair)
        {
            for (; index >= 0; index--)
            {
                if (GetAtIndex(index, out pair))
                    return true;
            }

            pair = default;
            return false;
        }

        public KeyValue GetFirst()
        {
            int i = 0;
            if (MoveNext(ref i, out KeyValue pair))
                return pair;
            throw new InvalidOperationException("Empty collection");
        }

        public KeyValue GetLast()
        {
            int i = ListCount - 1;
            if (MovePrevious(ref i, out KeyValue pair))
                return pair;
            throw new InvalidOperationException("Empty collection");
        }

        public KeyValue RemoveLast()
        {
            int i = ListCount - 1;
            if (MovePrevious(ref i, out KeyValue pair))
            {
                Remove(pair.Key);
                List.RemoveRangeFromEnd(i); // also remove from List
                return pair;
            }

            throw new InvalidOperationException("Empty collection");
        }

        public void EnsureCapacity(int capacity)
        {
            Dict.EnsureCapacity(capacity);
            List.EnsureCapacity(capacity);
        }

        public void TrimExcess()
        {
            Dict.TrimExcess();
            List.TrimExcess();
        }

        // not ordered
        public void CopyKeysTo(TKey[] array, int index)
        {
            Dict.Keys.CopyTo(array, index);
        }

        // not ordered
        public IEnumerable<TKey> GetKeys()
        {
            return Dict.Keys;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dict).Contains(item);
        }

        // not ordered ?
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dict).Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)Dict).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Dict).GetEnumerator();
        }
    }
}
