using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Implements <see cref="IConfigProvider"/> using <see cref="PlayerPrefs"/>.
    /// </summary>
    public class PlayerPrefsConfigProvider : MonoBehaviour, IConfigProvider
    {
        readonly HashSet<string> m_knownKeys = new(StringComparer.OrdinalIgnoreCase);


        IEnumerable<string> IConfigProvider.GetKnownKeys()
        {
            return m_knownKeys;
        }

        string IConfigProvider.GetProperty(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return null;
            m_knownKeys.Add(key);
            return PlayerPrefs.GetString(key, null);
        }

        bool IConfigProvider.RemoveProperty(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return false;
            PlayerPrefs.DeleteKey(key);
            m_knownKeys.Remove(key);
            return true;
        }

        void IConfigProvider.Save()
        {
            PlayerPrefs.Save();
        }

        void IConfigProvider.Clear()
        {
            PlayerPrefs.DeleteAll();
            m_knownKeys.Clear();
        }

        void IConfigProvider.SetProperty(string key, string value)
        {
            m_knownKeys.Add(key);
            PlayerPrefs.SetString(key, value);
        }
    }
}
