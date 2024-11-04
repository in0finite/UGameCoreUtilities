using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class ComponentPoolList<T> : IStatsCollectable
        where T : Component
    {
        readonly HashSetAndList<T> PooledObjects = new();
        public GameObject PrefabGameObject;
        public Transform ParentTransform;

        public bool ActivateWhenRenting = true;
        public bool SetParentWhenGettingFromPool = true;

        public bool KeepTrackOfRentedObjects = false;
        HashSet<T> RentedObjects = null;

        public int NumPooledObjects => PooledObjects.Count;
        public ulong NumCreations { get; private set; } = 0;
        public ulong NumPoolRents { get; private set; } = 0;
        public ulong NumPoolReturns { get; private set; } = 0;



        public T GetOrCreate()
        {
            T obj = PooledObjects.RemoveFromEndUntilAliveObject();
            if (obj != null)
            {
                if (SetParentWhenGettingFromPool)
                    obj.transform.SetParent(ParentTransform, true);
                NumPoolRents++;
            }
            else
            {
                obj = Object.Instantiate(PrefabGameObject, ParentTransform).GetComponentOrThrow<T>();
                NumCreations++;
            }
            
            if (ActivateWhenRenting)
                obj.gameObject.SetActive(true);

            if (KeepTrackOfRentedObjects)
            {
                RentedObjects ??= new HashSet<T>();
                RentedObjects.AddOrThrow(obj);
            }

            return obj;
        }

        public T Rent() => GetOrCreate();

        public void ReturnToPool(T obj)
        {
            if (null == obj)
                throw new System.ArgumentNullException();

            PooledObjects.Add(obj);
            RentedObjects?.Remove(obj);

            NumPoolReturns++;

            obj.gameObject.SetActive(false);
        }

        public void ReturnMultipleToPool(IReadOnlyList<T> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                ReturnToPool(list[i]);
        }

        /// <summary>
        /// Works only if <see cref="KeepTrackOfRentedObjects"/> is true.
        /// </summary>
        public void ReturnAllRentedObjects()
        {
            if (RentedObjects == null)
                return;

            using PooledArray<T> pooledArray = new(RentedObjects.Count);

            RentedObjects.CopyTo(pooledArray.Array);

            System.Span<T> span = pooledArray.Span;

            for (int i = 0; i < span.Length; i++)
            {
                T component = span[i];

                if (component != null) // don't try to return dead objects
                    ReturnToPool(component);
                else
                    RentedObjects.Remove(component); // dead object, don't keep track of him anymore
            }

            span.Clear(); // release references from rented array
        }
    }
}
