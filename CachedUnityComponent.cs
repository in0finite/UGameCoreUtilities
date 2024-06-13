using UnityEngine;

namespace UGameCore.Utilities
{
    public struct CachedUnityComponent<T>
        where T : Component
    {
        T m_component;
        bool m_isCached;

        public Component Owner { get; init; }
        public bool DontFailIfNotExists { get; init; }

        public T Component
        {
            get
            {
                if (m_isCached)
                    return m_component;

                m_component = this.DontFailIfNotExists
                    ? this.Owner.GetComponent<T>()
                    : this.Owner.GetComponentOrThrow<T>();

                m_isCached = true;

                return m_component;
            }
        }
    }
}
