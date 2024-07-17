using UnityEngine;

namespace UGameCore.Utilities
{
    public struct ExistableUnityObject<T>
        where T : Object
    {
        public T Object;
        public bool Exists;

        public ExistableUnityObject(T obj)
        {
            this.Object = obj;
            this.Exists = obj != null;
        }
    }
}
