using System;
using System.Collections;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    public class HashSetAndList<TKey> : ICollection<TKey>, IReadOnlyCollection<TKey>
    {
        readonly DictionaryAndList<TKey, byte> DictAndList;
        readonly Predicate<DictionaryAndList<TKey, byte>.KeyValue> RemoveAllPredicate;
        Predicate<TKey> UserRemoveAllPredicate;

        public int Count => DictAndList.Count;
        public int ListCount => DictAndList.ListCount;

        public bool IsReadOnly => false;


        public HashSetAndList(int capacity, IEqualityComparer<TKey> comparer)
        {
            DictAndList = new(capacity, comparer);
            RemoveAllPredicate = pair => UserRemoveAllPredicate(pair.Key);
        }

        public HashSetAndList(int capacity)
            : this(capacity, EqualityComparer<TKey>.Default)
        {
        }

        public HashSetAndList()
            : this(16)
        {
        }

        public void Add(TKey item)
        {
            DictAndList.Add(item, default);
        }

        public void AddRange(IEnumerable<TKey> items)
        {
            foreach (TKey item in items)
                Add(item);
        }

        public void Clear()
        {
            DictAndList.Clear();
        }

        public bool Contains(TKey item)
        {
            return DictAndList.ContainsKey(item);
        }

        public void CopyTo(TKey[] array, int arrayIndex)
        {
            DictAndList.CopyKeysTo(array, arrayIndex);
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            return DictAndList.GetKeys().GetEnumerator();
        }

        public bool Remove(TKey item)
        {
            return DictAndList.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return DictAndList.GetKeys().GetEnumerator();
        }

        public bool GetAtIndex(int index, out TKey value)
        {
            bool bGet = DictAndList.GetAtIndex(index, out var pair);
            value = pair.Key;
            return bGet;
        }

        public bool MoveNext(ref int index, out TKey value)
        {
            bool bMove = DictAndList.MoveNext(ref index, out var pair);
            value = pair.Key;
            return bMove;
        }

        public TKey GetFirst()
        {
            return DictAndList.GetFirst().Key;
        }

        public TKey RemoveLast()
        {
            return DictAndList.RemoveLast().Key;
        }

        /// <summary>
        /// Bulk-remove all entries from the <see cref="List{T}"/> which are not contained in the <see cref="HashSet{T}"/>.
        /// </summary>
        public void ConsolidateList()
        {
            DictAndList.ConsolidateList();
        }

        /// <summary>
        /// Consolidates, if difference between <see cref="HashSet{T}.Count"/> and <see cref="List{T}.Count"/> is higher than specified value.
        /// </summary>
        public void ConsolidateListIfDifference(int diff)
        {
            if (Math.Abs(DictAndList.ListCount - DictAndList.Count) > diff)
                DictAndList.ConsolidateList();
        }

        /// <summary>
        /// Bulk-remove all entries which pass specified <see cref="Predicate{KeyValue}"/>.
        /// This function is not re-entrable.
        /// After bulk-removal, number of entries in <see cref="List{T}"/> and <see cref="HashSet{T}"/> will be equal.
        /// </summary>
        public void RemoveAll(Predicate<TKey> predicate)
        {
            UserRemoveAllPredicate = predicate;
            DictAndList.RemoveAll(RemoveAllPredicate);
            UserRemoveAllPredicate = null;
        }
    }
}
