using System.Runtime.CompilerServices;
using UnityEngine;

namespace UGameCore.Utilities
{
    public struct CachedGameObject
    {
        bool m_isCached;

        GameObject m_gameObject;
        public GameObject GameObject
        {
            get
            {
                if (m_isCached)
                    return m_gameObject;
                this.EnsureCached();
                return m_gameObject;
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

        bool m_isActiveSelfCached;
        public bool IsActiveSelfCached
        {
            get
            {
                this.EnsureCached();
                this.ThrowIfNull();
                return m_isActiveSelfCached;
            }
        }

        int m_layerCached;
        public int LayerCached
        {
            get
            {
                this.EnsureCached();
                this.ThrowIfNull();
                return m_layerCached;
            }
        }

        bool m_isAliveCached;
        public bool IsAliveCached
        {
            get
            {
                this.EnsureCached();
                return m_isAliveCached;
            }
        }

        public Component Owner { get; init; }


        public CachedGameObject(Component owner)
            : this()
        {
            this.Owner = owner;
        }

        public CachedGameObject(GameObject go)
            : this(go != null ? go.transform : null)
        {
        }

        void EnsureCached()
        {
            if (m_isCached)
                return;

            bool isAlive = this.Owner != null;

            m_gameObject = isAlive ? this.Owner.gameObject : null;
            m_transform = isAlive ? this.Owner.transform : null;

            m_isAliveCached = isAlive;
            m_isActiveSelfCached = isAlive && m_gameObject.activeSelf;
            m_layerCached = isAlive ? m_gameObject.layer : 0;

            m_isCached = true;
        }

        public void SetActiveCached(bool isActive)
        {
            this.EnsureCached();
            this.ThrowIfNull();

            if (m_isActiveSelfCached == isActive)
                return;

            m_gameObject.SetActive(isActive);

            m_isActiveSelfCached = isActive;
        }

        public void SetLayerCached(int layer)
        {
            this.EnsureCached();
            this.ThrowIfNull();

            if (m_layerCached == layer)
                return;

            m_gameObject.layer = layer;

            m_layerCached = layer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly void ThrowIfNull()
        {
            if (!m_isAliveCached)
                throw new System.NullReferenceException($"GameObject is not alive");
        }

        public void Invalidate()
        {
            m_isCached = false;
            m_gameObject = null;
            m_transform = null;
        }
    }
}
