using System.IO;

namespace UGameCore.Utilities
{
    public class DefaultFileSystemAccessor : IFileSystemAccessor
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public string[] DirectoryGetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetDirectories(path, searchPattern, searchOption);
        }

        public string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public Stream FileOpen(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return File.Open(path, mode, access, share);
        }
    }
}
