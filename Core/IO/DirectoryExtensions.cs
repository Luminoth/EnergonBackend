using System.Threading.Tasks;

namespace System.IO
{
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Creates the directory asynchronously.
        /// </summary>
        /// <param name="path">The directory path to create.</param>
        public static Task CreateDirectoryAsync(string path)
        {
            return Task.Run(() => Directory.CreateDirectory(path));
        }
    }
}
