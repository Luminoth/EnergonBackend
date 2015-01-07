using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Core.Util
{
    // TODO: add a lock to this
    public sealed class MemoryBuffer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MemoryBuffer));

        public readonly Stream Buffer;
        public int Capacity { get { return ((MemoryStream)Buffer).Capacity; } }
        public int Length { get { return (int)Buffer.Length; } }
        public int Position { get { return (int)Buffer.Position; } }
        public int Remaining { get { return Length - Position; } }
        public bool HasRemaining { get { return Remaining > 0; } }

        public MemoryBuffer()
        {
            Buffer = new MemoryStream();
        }

        public MemoryBuffer(byte[] data)
        {
            Buffer = new MemoryStream(data);
        }

        public MemoryBuffer(byte[] data, int index, int count)
        {
            Buffer = new MemoryStream(data, index, count);
        }

        public MemoryBuffer(int capacity)
        {
            Buffer = new MemoryStream(capacity);
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                Buffer.Dispose();
            }
        }
#endregion

        public async Task WriteAsync(byte[] value, int offset, int count)
        {
            await Buffer.WriteAsync(value, offset, count).ConfigureAwait(false);
        }

        public async Task<int> PeekAsync(byte[] value, int offset, int count)
        {
            int read = await Buffer.ReadAsync(value, offset, count).ConfigureAwait(false);
            Buffer.Seek(-read, SeekOrigin.Current);
            return read;
        }

        public async Task<int> ReadAsync(byte[] value, int offset, int count)
        {
            return await Buffer.ReadAsync(value, offset, count).ConfigureAwait(false);
        }

        public void Clear()
        {
            Buffer.Position = 0;
            Buffer.SetLength(0);
        }

        public async Task CompactAsync()
        {
            byte[] buffer = ToArray();
            int position = (int)Position;

            Clear();
            await WriteAsync(buffer, position, buffer.Length - position).ConfigureAwait(false);
        }

        public void Flip()
        {
            Buffer.SetLength(Position);
            Buffer.Position = 0;
        }

        public void Reset()
        {
            Buffer.Position = Length;
        }

        public byte[] ToArray()
        {
            return ((MemoryStream)Buffer).ToArray();
        }
    }
}
