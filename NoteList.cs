using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Script that can store a list of general purpose notes/info.
    /// </summary>
    public class NoteList : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string title = string.Empty;
            [TextArea(1, 15)]
            public string text = string.Empty;
        }

        public string description = string.Empty;

        public Entry[] notes = Array.Empty<Entry>();

        public void SetNotes(IEnumerable<(string title, string text)> notes)
        {
            this.notes = notes.Select(_ => new Entry { title = _.title, text = _.text }).ToArray();
        }

        public void SetNotes(string description, IEnumerable<(string title, string text)> notes)
        {
            this.description = description;
            this.SetNotes(notes);
        }
    }
}
