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

        //public TKey RemoveFirst()
        //{

        //}

        public TKey RemoveLast()
        {
            return DictAndList.RemoveLast().Key;
        }

        /// <summary>
        /// Remove all entries from the List, which are not contained in the HashSet.
        /// </summary>
        public void ConsolidateList()
        {
            DictAndList.ConsolidateList();
        }

        /// <summary>
        /// Consolidates, if difference between Dictionary.Count and List.Count is higher than specified value.
        /// </summary>
        public void ConsolidateListIfDifference(int diff)
        {
            if (Math.Abs(DictAndList.ListCount - DictAndList.Count) > diff)
                DictAndList.ConsolidateList();
        }

        /// <summary>
        /// Note: this function is not re-entrable.
        /// </summary>
        public void RemoveAll(Predicate<TKey> predicate)
        {
            UserRemoveAllPredicate = predicate;
            DictAndList.RemoveAll(RemoveAllPredicate);
            UserRemoveAllPredicate = null;
        }
    }
}
