using System.Collections.Generic;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides configuration, in terms of properties that can be loaded/stored based on their key.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Gets property value from configuration as string. Returns null if there is no such key.
        /// </summary>
        string GetProperty(string key);

        /// <summary>
        /// Sets property as string value.
        /// </summary>
        void SetProperty(string key, string value);

        bool RemoveProperty(string key);

        /// <summary>
        /// Gets keys of all properties stored in configuration.
        /// </summary>
        IEnumerable<string> GetKeys();

        /// <summary>
        /// Saves configuration to permanent storage.
        /// </summary>
        void Save();
    }
}