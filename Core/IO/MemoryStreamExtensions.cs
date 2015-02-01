using System.Threading.Tasks;

namespace System.IO
{
    public static class MemoryStreamExtensions
    {
        public static async Task CompactAsync(this MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            long position = stream.Position;

            stream.Clear();
            await stream.WriteAsync(buffer, (int)position, (int)(buffer.Length - position)).ConfigureAwait(false);
        }
    }
}
