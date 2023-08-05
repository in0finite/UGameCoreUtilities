using System.Collections;

namespace UGameCore.Utilities
{
    public interface ISelectFolderDialogHandler
    {
        string Select(string title, string folder, string defaultName);

        IEnumerator SelectAsync(Ref<string> resultRef, string title, string folder, string defaultName);
    }
}
