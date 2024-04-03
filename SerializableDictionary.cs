using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        [SerializeField] List<SerializablePair<TKey, TValue>> m_list;

        Dictionary<TKey, TValue> m_dict;

        readonly IEqualityComparer<TKey> m_comparer;

        readonly bool m_throwOnDuplicates;

        public int Count
        {
            get
            {
                this.EnsureDictBuilt();
                return m_dict.Count;
            }
        }


        public SerializableDictionary()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public SerializableDictionary(IEqualityComparer<TKey> comparer)
            : this(4, comparer)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer)
            : this(capacity, comparer, true)
        {
        }

        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer, bool throwOnDuplicates)
        {
            m_list = new(capacity);
            m_comparer = comparer;
            m_throwOnDuplicates = throwOnDuplicates;
        }

        void EnsureDictBuilt()
        {
            if (m_dict != null)
                return;

            m_dict = new Dictionary<TKey, TValue>(m_list.Count, m_comparer);

            foreach (var pair in m_list)
            {
                bool added = m_dict.TryAdd(pair.item1, pair.item2);
                if (!added && m_throwOnDuplicates)
                    throw new ArgumentException($"Failed to add duplicated item to {this.GetType().Name} : {pair.item1}");
            }
        }

        public bool ContainsKey(TKey key)
        {
            this.EnsureDictBuilt();
            return m_dict.ContainsKey(key);
        }

        public TValue this[TKey key] { get => this.Get(key); set => this.AddOrReplace(key, value, true); }

        public TValue Get(TKey key)
        {
            this.EnsureDictBuilt();
            return m_dict[key];
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            this.EnsureDictBuilt();
            return m_dict.TryGetValue(key, out value);
        }

        public void Add(TKey key, TValue value)
        {
            this.EnsureDictBuilt();

            m_dict.Add(key, value);
            m_list.Add(new SerializablePair<TKey, TValue>(key, value));
        }

        public void AddOrReplace(TKey key, TValue value, bool saveChanges)
        {
            this.EnsureDictBuilt();

            var newPair = new SerializablePair<TKey, TValue>(key, value);

            if (m_dict.ContainsKey(key))
            {
                if (saveChanges)
                {
                    int index = m_list.FindIndex(pair => m_comparer.Equals(key, pair.item1));

                    if (index < 0) // should not happen, unless List was modified in Inspector
                        m_list.Add(newPair);
                    else
                        m_list[index] = newPair;
                }
                
                m_dict[key] = value;
            }
            else
            {
                m_dict.Add(key, value);
                if (saveChanges)
                    m_list.Add(newPair);
            }
        }

        public void Clear()
        {
            m_list.Clear();
            m_dict?.Clear();
        }

        /// <summary>
        /// Save changes made to <see cref="Dictionary{TKey, TValue}"/> to underlying <see cref="List{T}"/>.
        /// </summary>
        public void SaveChanges()
        {
            if (null == m_dict) // no changes made
                return;

            m_list.Clear();
            m_list.AddRange(m_dict.Select(pair => new SerializablePair<TKey, TValue>(pair.Key, pair.Value)));
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            this.EnsureDictBuilt();
            return m_dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            this.EnsureDictBuilt();
            return m_dict.GetEnumerator();
        }
    }
}
