using System.Collections;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Allows user to select a folder through a dialog.
    /// </summary>
    public interface ISelectFolderDialog
    {
        IEnumerator SelectAsync(Ref<string> resultRef, string title, string folder, string defaultName);
    }
}
