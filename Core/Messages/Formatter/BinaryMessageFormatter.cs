using System;
using System.IO;
using System.Net;
using System.Text;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public sealed class BinaryMessageFormatter : IMessageFormatter
    {
        public BinaryMessageFormatter()
        {
        }

        public void WriteString(string value, Stream stream)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length, stream);
            Write(bytes, 0, bytes.Length, stream);
        }

        public void WriteByte(byte value, Stream stream)
        {
            Write(new byte[1] { value }, 0, 1, stream);
        }

        public void WriteBool(bool value, Stream stream)
        {
            WriteByte((byte)(value ? 1 : 0), stream);
        }

        public void WriteInt(int value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            Write(bytes, 0, bytes.Length, stream);
        }

        public void WriteLong(long value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            Write(bytes, 0, bytes.Length, stream);
        }

        public void WriteFloat(float value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            Write(bytes, 0, bytes.Length, stream);
        }

        public void WriteDouble(double value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            Write(bytes, 0, bytes.Length, stream);
        }

        public void Write(byte[] value, int offset, int count, Stream stream)
        {
            stream.Write(value, offset, count);
        }

        public string ReadString(Stream stream)
        {
            int length = ReadInt(stream);
            byte[] bytes = new byte[length];
            Read(bytes, 0, bytes.Length, stream);
            return Encoding.UTF8.GetString(bytes);
        }

        public byte ReadByte(Stream stream)
        {
            byte[] bytes = new byte[1];
            Read(bytes, 0, bytes.Length, stream);
            return bytes[0];
        }

        public bool ReadBool(Stream stream)
        {
            return 0 != ReadByte(stream);
        }

        public int ReadInt(Stream stream)
        {
            byte[] bytes = new byte[4];
            Read(bytes, 0, bytes.Length, stream);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }

        public long ReadLong(Stream stream)
        {
            byte[] bytes = new byte[8];
            Read(bytes, 0, bytes.Length, stream);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, 0));
        }

        public float ReadFloat(Stream stream)
        {
            byte[] bytes = new byte[4];
            Read(bytes, 0, bytes.Length, stream);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble(Stream stream)
        {
            byte[] bytes = new byte[8];
            Read(bytes, 0, bytes.Length, stream);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        public void Read(byte[] value, int offset, int count, Stream stream)
        {
            stream.Read(value, offset, count);
        }
    }
}
