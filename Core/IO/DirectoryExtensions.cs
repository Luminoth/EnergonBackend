using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>
    /// Useful extensions to the System.IO.Directory class.
    /// </summary>
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
