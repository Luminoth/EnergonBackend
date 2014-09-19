using System;
using System.IO;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Util
{
    public sealed class MemoryBuffer
    {
        private MemoryStream _buffer;

        public Stream Stream { get { return _buffer; } }
        public int Capacity { get { return _buffer.Capacity; } }
        public long Length { get { return _buffer.Length; } }
        public long Position { get { return _buffer.Position; } }
        public long Remaining { get { return Length - Position; } }
        public bool HasRemaining { get { return Remaining > 0; } }

        public MemoryBuffer()
        {
            _buffer = new MemoryStream();
        }

        public MemoryBuffer(byte[] data)
        {
            _buffer = new MemoryStream(data);
        }

        public MemoryBuffer(byte[] data, int index, int count)
        {
            _buffer = new MemoryStream(data, index, count);
        }

        public MemoryBuffer(int capacity)
        {
            _buffer = new MemoryStream(capacity);
        }

        public void Write(byte[] value, int offset, int count)
        {
            _buffer.Write(value, offset, count);
        }

        public void Read(byte[] value, int offset, int count)
        {
            _buffer.Read(value, offset, count);
        }

        public void Clear()
        {
            _buffer.Position = 0;
            _buffer.SetLength(0);
        }

        public void Compact()
        {
            byte[] buffer = ToArray();
            int position = (int)Position;

            Clear();
            Write(buffer, position, buffer.Length - position);
        }

        public void Flip()
        {
            _buffer.SetLength(Position);
            _buffer.Position = 0;
        }

        public void Reset()
        {
            _buffer.Position = Length;
        }

        public byte[] ToArray()
        {
            return _buffer.ToArray();
        }
    }
}
