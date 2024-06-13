using UnityEngine;

namespace UGameCore.Utilities
{
    public struct ExistableUnityObject<T>
        where T : Object
    {
        public T Object;
        public bool Exists;
    }
}
