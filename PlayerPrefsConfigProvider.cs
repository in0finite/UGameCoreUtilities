using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Implements <see cref="IConfigProvider"/> using <see cref="PlayerPrefs"/>.
    /// </summary>
    public class PlayerPrefsConfigProvider : MonoBehaviour, IConfigProvider
    {
        IEnumerable<string> IConfigProvider.GetKeys()
        {
            return Enumerable.Empty<string>();
        }

        string IConfigProvider.GetProperty(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return null;
            return PlayerPrefs.GetString(key, null);
        }

        bool IConfigProvider.RemoveProperty(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return false;
            PlayerPrefs.DeleteKey(key);
            return true;
        }

        void IConfigProvider.Save()
        {
            PlayerPrefs.Save();
        }

        void IConfigProvider.Clear()
        {
            PlayerPrefs.DeleteAll();
        }

        void IConfigProvider.SetProperty(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
    }
}
