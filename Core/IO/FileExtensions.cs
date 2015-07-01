using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    public static class FileExtensions
    {
        /// <summary>
        /// Moves a file asynchronously.
        /// </summary>
        /// <param name="sourceFileName">The path to the source file.</param>
        /// <param name="destFileName">The path to the dest file.</param>
        public static Task MoveAsync(string sourceFileName, string destFileName)
        {
            return Task.Run(() => File.Move(sourceFileName, destFileName));
        }

        /// <summary>
        /// Deletes a file asynchronous.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        public static Task DeleteAsync(string path)
        {
            return Task.Run(() => File.Delete(path));
        }
    }
}
