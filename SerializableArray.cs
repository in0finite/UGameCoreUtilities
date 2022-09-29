using System;
using System.Collections.Generic;

namespace UGameCore.Utilities
{
    [Serializable]
    public struct SerializableArray<T>
    {
        public T[] array;

        public SerializableArray(int length)
        {
            this.array = length != 0
                ? new T[length]
                : Array.Empty<T>();
        }

        public SerializableArray(ICollection<T> collection)
            : this(collection.Count)
        {
            collection.CopyTo(this.array, 0);
        }
    }
}
