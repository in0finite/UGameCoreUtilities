using System.IO;

namespace UGameCore.Utilities
{
    /// <summary>
    /// Interface that can be used to abstractize access to file-system.
    /// </summary>
    public interface IFileSystemAccessor
    {
        bool FileExists(string path);

        Stream FileOpen(string path, FileMode mode, FileAccess access, FileShare share);

        bool DirectoryExists(string path);

        string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption);

        string[] DirectoryGetDirectories(string path, string searchPattern, SearchOption searchOption);
    }

    public static class FileSystemAccessorExtensions
    {
        public static Stream FileOpenRead(this IFileSystemAccessor fs, string path)
            => fs.FileOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
