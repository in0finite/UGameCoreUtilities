using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class ComponentPoolList<T> : IStatsCollectable
        where T : Component
    {
        readonly List<T> PooledObjects = new();
        public GameObject PrefabGameObject;
        public Transform ParentTransform;

        public bool ActivateWhenRenting = true;
        public bool SetParentWhenGettingFromPool = true;

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

            return obj;
        }

        public void ReturnToPool(T obj)
        {
            if (null == obj)
                throw new System.ArgumentNullException();

            obj.gameObject.SetActive(false);
            PooledObjects.Add(obj);
            NumPoolReturns++;
        }

        public void ReturnMultipleToPool(IReadOnlyList<T> list)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
                ReturnToPool(list[i]);
        }
    }
}
