using System.Collections;
using System.IO;

namespace UGameCore.Utilities
{
    public class DefaultFileSystemAccessorAsync : IFileSystemAccessorAsync
    {
        IEnumerator IFileSystemAccessorAsync.FileExistsAsync(Ref<bool> result, string path)
        {
            result.value = File.Exists(path);
            yield break;
        }

        IEnumerator IFileSystemAccessorAsync.FileOpenAsync(Ref<Stream> result, string path, FileMode mode, FileAccess access, FileShare share)
        {
            result.value = File.Open(path, mode, access, share);
            yield break;
        }

        IEnumerator IFileSystemAccessorAsync.DirectoryExistsAsync(Ref<bool> result, string path)
        {
            result.value = Directory.Exists(path);
            yield break;
        }

        IEnumerator IFileSystemAccessorAsync.DirectoryGetFilesAsync(Ref<string[]> result, string path, string searchPattern, SearchOption searchOption)
        {
            result.value = Directory.GetFiles(path, searchPattern, searchOption);
            yield break;
        }

        IEnumerator IFileSystemAccessorAsync.DirectoryGetDirectoriesAsync(Ref<string[]> result, string path, string searchPattern, SearchOption searchOption)
        {
            result.value = Directory.GetDirectories(path, searchPattern, searchOption);
            yield break;
        }
    }
}
