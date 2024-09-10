using System;
using System.Collections.Generic;
using System.Linq;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Collection that can hold a single object without allocating memory, or multiple objects using a <see cref="List{T}"/>.
    /// </summary>
    public struct ListOrSingleObject<T>
    {
        List<T> List;
        T SingleObject;
        bool HasSingleObject;


        public readonly int Count
        {
            get
            {
                if (HasSingleObject)
                    return 1;

                if (List == null)
                    return 0;

                return List.Count;
            }
        }

        public void Add(T obj)
        {
            if (HasSingleObject) // collection contains only 1 item
            {
                List = new List<T> { SingleObject, obj };

                SingleObject = default;
                HasSingleObject = false;

                return;
            }

            if (List == null) // collection is empty
            {
                SingleObject = obj;
                HasSingleObject = true;
                return;
            }

            // collection potentially has more than 1 item (but it could still be empty)

            List.Add(obj);
        }

        public void AddRange(IEnumerable<T> objects)
        {
            if (HasSingleObject)
            {
                List = new List<T> { SingleObject };

                List.AddRange(objects);

                SingleObject = default;
                HasSingleObject = false;

                return;
            }

            if (List == null)
                List = new List<T>(objects);
            else
                List.AddRange(objects);
        }

        public void Clear()
        {
            List?.Clear();
            SingleObject = default;
            HasSingleObject = false;
        }

        public T this[int index]
        {
            readonly get
            {
                if (HasSingleObject)
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return SingleObject;
                }

                if (List == null)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return List[index];
            }
            set
            {
                if (HasSingleObject)
                {
                    if (index != 0)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    SingleObject = value;
                    return;
                }

                if (List == null)
                    throw new ArgumentOutOfRangeException(nameof(index));

                List[index] = value;
            }
        }

        public T RemoveLast()
        {
            if (HasSingleObject)
            {
                HasSingleObject = false;
                T ret = SingleObject;
                SingleObject = default;
                return ret;
            }

            if (List == null || List.Count == 0)
                throw new InvalidOperationException("Collection is empty");

            int lastIndex = List.Count - 1;
            T last = List[lastIndex];
            List.RemoveAt(lastIndex);
            return last;
        }

        public readonly IEnumerable<T> AsEnumerable()
        {
            if (HasSingleObject)
                return new T[] { SingleObject };
            
            if (List != null)
                return List;

            return Array.Empty<T>();
        }

        public readonly T FirstOrDefault(Func<T, bool> predicate)
        {
            if (HasSingleObject)
                return predicate(SingleObject) ? SingleObject : default;
            
            if (List == null)
                return default;

            return List.FirstOrDefault(predicate);
        }
    }
}
