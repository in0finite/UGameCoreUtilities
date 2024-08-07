using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Dispatches input events (mouse, keyboard, button) to registered subscribers, via <see cref="UnityEvent"/>.
    /// </summary>
    public class InputEventsDispatcher : MonoBehaviour
    {
        public bool editMode = false;
        public bool skipWhenUIHasKeyboardFocus = true;
        public bool skipWhenUIHasFocus = false;

        public GameObject onlyWhenThisObjectIsSelected = null;

        public int[] mouseButtonsToCheck = Array.Empty<int>();
        public KeyCode[] keyCodesToCheck = Array.Empty<KeyCode>();
        public string[] buttonsToCheck = Array.Empty<string>();

        public UnityEvent<int> onMouseButtonPress = new();
        public UnityEvent<KeyCode> onKeyPress = new();
        public UnityEvent<string> onButtonPress = new();

        public List<SerializablePair<int, UnityEvent>> onSpecificMouseButtonPress = new();
        public List<SerializablePair<KeyCode, UnityEvent>> onSpecificKeyPress = new();
        public List<SerializablePair<string, UnityEvent>> onSpecificButtonPress = new();


        InputEventsDispatcher()
        {
            EditorApplicationEvents.Register(this);
        }

        void Update()
        {
            if (!this.editMode && !Application.isPlaying)
                return;

            if (this.onlyWhenThisObjectIsSelected != null && this.onlyWhenThisObjectIsSelected != F.UIFocusedObject())
                return;

            if (this.skipWhenUIHasKeyboardFocus && F.UIHasKeyboardFocus())
                return;

            if (this.skipWhenUIHasFocus && F.UIHasFocus())
                return;

            // dispatch

            foreach (int mouseButton in this.mouseButtonsToCheck)
            {
                if (Input.GetMouseButtonDown(mouseButton))
                {
                    this.onMouseButtonPress.Invoke(mouseButton);
                    this.onSpecificMouseButtonPress.Where(_ => _.item1 == mouseButton).ForEach(_ => _.item2?.Invoke());
                }
            }

            foreach (KeyCode keyCode in this.keyCodesToCheck)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    this.onKeyPress.Invoke(keyCode);
                    this.onSpecificKeyPress.Where(_ => _.item1 == keyCode).ForEach(_ => _.item2?.Invoke());
                }
            }

            foreach (string button in this.buttonsToCheck)
            {
                if (Input.GetButtonDown(button))
                {
                    this.onButtonPress.Invoke(button);
                    this.onSpecificButtonPress.Where(_ => _.item1 == button).ForEach(_ => _.item2?.Invoke());
                }
            }
        }

        [ContextMenu("Add all numbers")]
        void AddAllNumbers()
        {
            var list = new List<KeyCode>(this.keyCodesToCheck);

            for (KeyCode i = KeyCode.Alpha0; i <= KeyCode.Alpha9; i++)
                list.Add(i);

            for (KeyCode i = KeyCode.Keypad0; i <= KeyCode.Keypad9; i++)
                list.Add(i);

            this.keyCodesToCheck = list.Distinct().ToArray();

            EditorUtilityEx.MarkObjectAsDirty(this);
        }
    }
}
