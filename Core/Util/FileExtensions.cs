using System.Threading.Tasks;

namespace System.IO
{
    public static class FileExtensions
    {
        public static Task MoveAsync(string sourceFileName, string destFileName)
        {
            return Task.Run(() => File.Move(sourceFileName, destFileName));
        }

        public static Task DeleteAsync(string path)
        {
            return Task.Run(() => File.Delete(path));
        }
    }
}
