using System.Collections;
using UnityEngine;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Achieves user interaction with Editor dialogs.
    /// </summary>
    public class EditorDialogUserInteraction : IUserInteraction
    {
        public bool SupportsConfirm => Application.isEditor;

        public IEnumerator ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok, string cancel)
        {
            if (!Application.isEditor)
                throw new System.NotSupportedException("Only works in Editor");

#if UNITY_EDITOR
            bResultRef.value = UnityEditor.EditorUtility.DisplayDialog(title, message, ok, cancel);
#endif

            yield break;
        }

        public void ShowMessage(string title, string message)
        {
            if (!Application.isEditor)
                throw new System.NotSupportedException("Only works in Editor");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog(title, message, "Ok");
#endif
        }

        public IEnumerator ShowMessageAsync(string title, string message)
        {
            ShowMessage(title, message);
            yield break;
        }
    }
}
