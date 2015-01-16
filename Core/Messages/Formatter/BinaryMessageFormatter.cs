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
        public const string FormatterType = "binary";
        public string Type { get { return FormatterType; } }

        private Stream _stream;

        internal BinaryMessageFormatter()
        {
        }

        public void Attach(Stream stream)
        {
            _stream = stream;
        }

#region Writing
        public async Task FlushAsync()
        {
            await _stream.FlushAsync().ConfigureAwait(false);
        }

        public async Task StartDocumentAsync()
        {
            await StartElementAsync("message").ConfigureAwait(false);
        }

        public async Task EndDocumentAsync()
        {
            await EndElementAsync().ConfigureAwait(false);
        }

        public async Task StartElementAsync(string name)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task EndElementAsync()
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IMessageSerializable
        {
            await WriteAsync(name, values.Count).ConfigureAwait(false);
            foreach(T value in values) {
                await WriteAsync(value).ConfigureAwait(false);
            }
        }

        public async Task WriteAsync(IMessageSerializable value)
        {
            await StartElementAsync(value.Type).ConfigureAwait(false);
            await value.SerializeAsync(this).ConfigureAwait(false);
            await EndElementAsync().ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            await WriteAsync(name, bytes.Length).ConfigureAwait(false);
            await WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, byte value)
        {
            await WriteAsync(new byte[1] { value }, 0, 1).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, bool value)
        {
            await WriteAsync(name, (byte)(value ? 1 : 0)).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, int value)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, long value)
        {
            byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            await WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            await WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            await WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
        }

        public async Task WriteAsync(byte[] value, int offset, int count)
        {
            await _stream.WriteAsync(value, offset, count).ConfigureAwait(false);
        }
#endregion

#region Reading
        public async Task<List<T>> ReadListAsync<T>(string name) where T : IMessageSerializable, new()
        {
            List<T> values = new List<T>();

            int count = await ReadIntAsync(name).ConfigureAwait(false);
            for(int i=0; i<count; ++i) {
                values.Add(await ReadAsync<T>(name).ConfigureAwait(false));
            }
            return values;
        }

        public async Task<T> ReadAsync<T>(string name) where T : IMessageSerializable, new()
        {
            T value = new T();
            await value.DeSerializeAsync(this).ConfigureAwait(false);
            return value;
        }

        public async Task<string> ReadStringAsync(string name)
        {
            int length = await ReadIntAsync(name).ConfigureAwait(false);
            byte[] bytes = new byte[length];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<byte> ReadByteAsync(string name)
        {
            byte[] bytes = new byte[1];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return bytes[0];
        }

        public async Task<bool> ReadBoolAsync(string name)
        {
            return 0 != await ReadByteAsync(name).ConfigureAwait(false);
        }

        public async Task<int> ReadIntAsync(string name)
        {
            byte[] bytes = new byte[4];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }

        public async Task<long> ReadLongAsync(string name)
        {
            byte[] bytes = new byte[8];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, 0));
        }

        public async Task<float> ReadFloatAsync(string name)
        {
            byte[] bytes = new byte[4];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }

        public async Task<double> ReadDoubleAsync(string name)
        {
            byte[] bytes = new byte[8];
            await ReadAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            if(BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        public async Task ReadAsync(byte[] value, int offset, int count)
        {
            await _stream.ReadAsync(value, offset, count).ConfigureAwait(false);
        }
#endregion
    }
}
