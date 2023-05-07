using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Script that can store a general purpose notes/info.
    /// </summary>
    public class Note : MonoBehaviour
    {
        public string description = string.Empty;

        [TextArea(4, 15)]
        public string text = string.Empty;

        public void Set(string description, string text)
        {
            this.description = description;
            this.text = text;
        }
    }
}
