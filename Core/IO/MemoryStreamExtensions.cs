using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>
    /// Useful extensions to the System.IO.MemoryStream class.
    /// </summary>
    public static class MemoryStreamExtensions
    {
        /// <summary>
        /// Copies the bytes between the stream's current position and its length to the beginning of the stream
        /// and sets the stream's position to the end of the stream so that this method can be followed immediately by a write operation.
        /// Call after consuming data from the stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public static async Task CompactAsync(this MemoryStream stream)
        {
            byte[] buffer = stream.ToArray();
            long position = stream.Position;

            stream.Clear();

            await stream.WriteAsync(buffer, (int)position, (int)(buffer.Length - position)).ConfigureAwait(false);
        }
    }
}
