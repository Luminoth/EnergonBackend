﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Util
{
    public sealed class MemoryBuffer
    {
        public Stream Buffer { get; private set; }
        public int Capacity { get { return ((MemoryStream)Buffer).Capacity; } }
        public long Length { get { return Buffer.Length; } }
        public long Position { get { return Buffer.Position; } }
        public long Remaining { get { return Length - Position; } }
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

        public void Write(byte[] value, int offset, int count)
        {
            Buffer.Write(value, offset, count);
        }

        public void Read(byte[] value, int offset, int count)
        {
            Buffer.Read(value, offset, count);
        }

        public void Clear()
        {
            Buffer.Position = 0;
            Buffer.SetLength(0);
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