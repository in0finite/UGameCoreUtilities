using System;

namespace UGameCore.Utilities
{
    [Serializable]
    public struct SerializablePair<T1, T2>
    {
        public T1 item1;
        public T2 item2;
    }
}
