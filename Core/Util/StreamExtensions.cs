using System.Threading.Tasks;

namespace System.IO
{
    public static class MemoryStreamExtensions
    {
        public static long GetRemaining(this Stream stream)
        {
            return stream.Length - stream.Position;
        }

        public static bool HasRemaining(this Stream stream)
        {
            return stream.GetRemaining() > 0;
        }

        public static async Task WriteAsync(this Stream stream, byte value)
        {
            await stream.WriteAsync(new byte[1] { value }, 0, 1).ConfigureAwait(false);
        }

        public static async Task WriteAsync(this Stream stream, bool value)
        {
            await stream.WriteAsync((byte)(value ? 1 : 0)).ConfigureAwait(false);
        }

        public static async Task<int> PeekAsync(this Stream stream, byte[] value, int offset, int count)
        {
            int read = await stream.ReadAsync(value, offset, count).ConfigureAwait(false);
            stream.Seek(-read, SeekOrigin.Current);
            return read;
        }

        public static async Task<byte> ReadByteAsync(this Stream stream)
        {
            byte[] bytes = new byte[1];
            await stream.ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return bytes[0];
        }

        public static async Task<bool> ReadBoolAsync(this Stream stream)
        {
            return 0 != await stream.ReadByteAsync().ConfigureAwait(false);
        }

        public static void Clear(this Stream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
        }

        public static async Task CompactAsync(this MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            long position = stream.Position;

            stream.Clear();
            await stream.WriteAsync(buffer, (int)position, (int)(buffer.Length - position)).ConfigureAwait(false);
        }

        public static void Flip(this Stream stream)
        {
            stream.SetLength(stream.Position);
            stream.Position = 0;
        }

        public static void Reset(this Stream stream)
        {
            stream.Position = stream.Length;
        }
    }
}
