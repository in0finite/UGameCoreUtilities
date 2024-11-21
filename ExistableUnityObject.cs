using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Struct that caches "alive" state of Unity Object.
    /// </summary>
    public struct ExistableUnityObject<T>
        where T : Object
    {
        public T Object;
        public bool Exists;


        public ExistableUnityObject(T obj)
        {
            this.Exists = obj != null;
            this.Object = this.Exists ? obj : null; // don't store object if he is dead
        }

        public static implicit operator ExistableUnityObject<T>(T obj)
        {
            return new ExistableUnityObject<T>(obj);
        }

        public static implicit operator T(ExistableUnityObject<T> existableUnityObject)
        {
            return existableUnityObject.Object;
        }
    }
}
