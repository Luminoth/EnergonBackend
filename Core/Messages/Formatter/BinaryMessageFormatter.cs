using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public sealed class BinaryMessageFormatter : IMessageFormatter
    {
        public async Task WriteListAsync<T>(List<T> value, Stream stream) where T : IMessageSerializable
        {
            await WriteIntAsync(value.Count, stream).ConfigureAwait(false);
            value.ForEach(async (v) => await v.SerializeAsync(stream, this).ConfigureAwait(false));
        }

        public async Task WriteStringAsync(string value, Stream stream)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            await WriteIntAsync(bytes.Length, stream).ConfigureAwait(false);
            await WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
        }

        public async Task WriteByteAsync(byte value, Stream stream)
        {
            await WriteAsync(new byte[1] { value }, 0, 1, stream).ConfigureAwait(false);
        }

        public async Task WriteBoolAsync(bool value, Stream stream)
        {
            await WriteByteAsync((byte)(value ? 1 : 0), stream).ConfigureAwait(false);
        }

        public async Task WriteIntAsync(int value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
        }

        public async Task WriteLongAsync(long value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
        }

        public async Task WriteFloatAsync(float value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            await WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
        }

        public async Task WriteDoubleAsync(double value, Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            await WriteAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
        }

        public async Task WriteAsync(byte[] value, int offset, int count, Stream stream)
        {
            await stream.WriteAsync(value, offset, count).ConfigureAwait(false);
        }

        public async Task<List<T>> ReadListAsync<T>(Stream stream) where T : IMessageSerializable, new()
        {
            List<T> values = new List<T>();

            int count = await ReadIntAsync(stream).ConfigureAwait(false);
            for(int i=0; i<count; ++i) {
                T value = new T();
                await value.DeSerializeAsync(stream, this).ConfigureAwait(false);
                values.Add(value);
            }
            return values;
        }

        public async Task<string> ReadStringAsync(Stream stream)
        {
            int length = await ReadIntAsync(stream).ConfigureAwait(false);
            byte[] bytes = new byte[length];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<byte> ReadByteAsync(Stream stream)
        {
            byte[] bytes = new byte[1];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            return bytes[0];
        }

        public async Task<bool> ReadBoolAsync(Stream stream)
        {
            return 0 != await ReadByteAsync(stream).ConfigureAwait(false);
        }

        public async Task<int> ReadIntAsync(Stream stream)
        {
            byte[] bytes = new byte[4];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }

        public async Task<long> ReadLongAsync(Stream stream)
        {
            byte[] bytes = new byte[8];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, 0));
        }

        public async Task<float> ReadFloatAsync(Stream stream)
        {
            byte[] bytes = new byte[4];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        public async Task<double> ReadDoubleAsync(Stream stream)
        {
            byte[] bytes = new byte[8];
            await ReadAsync(bytes, 0, bytes.Length, stream).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        public async Task ReadAsync(byte[] value, int offset, int count, Stream stream)
        {
            await stream.ReadAsync(value, offset, count).ConfigureAwait(false);
        }
    }
}
