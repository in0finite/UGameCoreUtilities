using System.Collections;
using UnityEngine;

namespace UGameCore.Utilities
{
    public class DefaultUserInteraction : IUserInteraction
    {
        public bool LogMessages { get; set; } = false;
        public bool UseEditorDialogs { get; set; } = true;

        
        void IUserInteraction.ShowMessage(string title, string message)
        {
            if (this.LogMessages)
                Debug.Log($"UserInteraction message: {title}\n{message}");

#if UNITY_EDITOR
            if (this.UseEditorDialogs)
            {
                UnityEditor.EditorUtility.DisplayDialog(title, message, "Ok");
            }
#endif
        }

        IEnumerator IUserInteraction.ShowMessageAsync(string title, string message)
        {
            ((IUserInteraction)this).ShowMessage(title, message);
            yield break;
        }

        bool IUserInteraction.SupportsConfirm
        {
            get
            {
#if UNITY_EDITOR
                if (this.UseEditorDialogs)
                    return true;
#endif
                return false;
            }
        }

        IEnumerator IUserInteraction.ConfirmAsync(Ref<bool> bResultRef, string title, string message, string ok, string cancel)
        {
            if (this.LogMessages)
                Debug.Log($"UserInteraction confirm: {title}\n{message}");

#if UNITY_EDITOR
            if (this.UseEditorDialogs)
            {
                bResultRef.value = UnityEditor.EditorUtility.DisplayDialog(title, message, ok, cancel);
                yield break;
            }
#endif

            throw new System.NotSupportedException("Confirm not supported");
        }
    }
}
