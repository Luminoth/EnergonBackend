using System.Threading.Tasks;

namespace System.IO
{
    public static class DirectoryExtensions
    {
        public static Task CreateDirectoryAsync(string path)
        {
            return Task.Run(() => Directory.CreateDirectory(path));
        }
    }
}
