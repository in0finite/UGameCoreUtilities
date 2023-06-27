using System.Collections;
using System.IO;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Interface that can be used to abstractize access to file-system, in asynchronous way.
    /// </summary>
    public interface IFileSystemAccessorAsync
    {
        IEnumerator FileExistsAsync(Ref<bool> result, string path);

        IEnumerator FileOpenAsync(Ref<Stream> result, string path, FileMode mode, FileAccess access, FileShare share);

        IEnumerator DirectoryExistsAsync(Ref<bool> result, string path);

        IEnumerator DirectoryGetFilesAsync(Ref<string[]> result, string path, string searchPattern, SearchOption searchOption);

        IEnumerator DirectoryGetDirectoriesAsync(Ref<string[]> result, string path, string searchPattern, SearchOption searchOption);
    }

    public static class FileSystemAccessorAsyncExtensions
    {
        public static IEnumerator FileOpenReadAsync(this IFileSystemAccessorAsync fs, Ref<Stream> result, string path)
            => fs.FileOpenAsync(result, path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
