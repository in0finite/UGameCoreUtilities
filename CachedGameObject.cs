using UnityEngine;

namespace UGameCore.Utilities
{
    public struct CachedGameObject
    {
        GameObject m_gameObject;
        bool m_isCached;

        bool? m_isActiveSelfCached;
        public bool IsActiveSelfCached => m_isActiveSelfCached ??= this.GameObject.activeSelf;

        public Component Owner { get; init; }

        public GameObject GameObject
        {
            get
            {
                if (m_isCached)
                    return m_gameObject;

                m_gameObject = this.Owner.gameObject;

                m_isCached = true;
                m_isActiveSelfCached = m_gameObject.activeSelf;

                return m_gameObject;
            }
        }

        public CachedGameObject(Component owner)
            : this()
        {
            this.Owner = owner;
        }

        public void SetActiveCached(bool isActive)
        {
            if (m_isActiveSelfCached.HasValue && m_isActiveSelfCached.Value == isActive)
                return;

            this.GameObject.SetActive(isActive);

            m_isActiveSelfCached = isActive;
        }
    }
}
