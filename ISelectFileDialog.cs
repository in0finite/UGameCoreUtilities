using System.Collections;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Allows user to select a file through a dialog.
    /// </summary>
    public interface ISelectFileDialog
    {
        IEnumerator SelectAsync(Ref<string> resultRef, string title, string directory, string extension);
    }
}
