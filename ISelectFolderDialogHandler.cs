using System.Collections;

namespace UGameCore.Utilities
{
    public interface ISelectFolderDialogHandler
    {
        string Select(string title, string folder, string defaultName)
        {
            var resultRef = new Ref<string>();
            SelectAsync(resultRef, title, folder, defaultName).EnumerateToEnd();
            return resultRef.value;
        }

        IEnumerator SelectAsync(Ref<string> resultRef, string title, string folder, string defaultName);
    }
}
