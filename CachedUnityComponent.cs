using System.Runtime.CompilerServices;
using UnityEngine;

namespace UGameCore.Utilities
{
    public struct CachedUnityComponent<T>
        where T : Component
    {
        readonly T m_component;
        bool m_isCached;

        public readonly CachedGameObject CachedGameObject;

        public readonly T Component => m_component;

        bool m_isAliveCached;
        public bool IsAliveCached
        {
            get
            {
                if (m_isCached)
                    return m_isAliveCached;
                this.EnsureCached();
                return m_isAliveCached;
            }
        }

        Transform m_transform;
        public Transform Transform
        {
            get
            {
                this.EnsureCached();
                this.ThrowIfNull();
                return m_transform;
            }
        }


        public CachedUnityComponent(T component)
            : this()
        {
            m_component = component;
            this.CachedGameObject = new CachedGameObject(component);
        }

        void EnsureCached()
        {
            if (m_isCached)
                return;

            m_isAliveCached = m_component != null;
            m_transform = m_isAliveCached ? m_component.transform : null;

            m_isCached = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ThrowIfNull()
        {
            if (!m_isAliveCached)
                throw new System.NullReferenceException($"{typeof(T).Name} component is not alive");
        }

        public void Invalidate()
        {
            m_isCached = false;
            m_transform = null;
            this.CachedGameObject.Invalidate();
        }
    }
}
