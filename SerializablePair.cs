using System;

namespace UGameCore.Utilities
{
    [Serializable]
    public struct SerializablePair<T1, T2>
    {
        public T1 item1;
        public T2 item2;

        public SerializablePair(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }
    }
}
