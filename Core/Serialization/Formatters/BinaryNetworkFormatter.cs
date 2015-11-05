using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EnergonSoftware.Core.IO;

namespace EnergonSoftware.Core.Serialization.Formatters
{
    /// <summary>
    /// Formats objects in a network byte order binary format.
    /// </summary>
    public class BinaryNetworkFormatter : IFormatter
    {
        /// <summary>
        /// The formatter type.
        /// </summary>
        public const string FormatterType = "BinaryNetwork";

        public string Type => FormatterType;

        private Stream _stream;

        public void Attach(Stream stream)
        {
            _stream = stream;
        }

        public async Task FlushAsync()
        {
            await _stream.FlushAsync().ConfigureAwait(false);
        }

        public async Task BeginAsync(string documentRoot)
        {
            await StartElementAsync(documentRoot).ConfigureAwait(false);
        }

        public async Task FinishAsync()
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

#region Write
        public async Task WriteAsync(string name, string value)
        {
            await _stream.WriteNetworkAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, byte value)
        {
            await _stream.WriteAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, bool value)
        {
            await _stream.WriteAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, int value)
        {
            await _stream.WriteNetworkAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, long value)
        {
            await _stream.WriteNetworkAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, float value)
        {
            await _stream.WriteNetworkAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, double value)
        {
            await _stream.WriteNetworkAsync(value).ConfigureAwait(false);
        }

        public async Task WriteAsync(string name, byte[] value, int offset, int count)
        {
            await _stream.WriteAsync(value, offset, count).ConfigureAwait(false);
        }

        public async Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IFormattable
        {
            await _stream.WriteNetworkAsync(values.Count).ConfigureAwait(false);
            foreach(T value in values) {
                await WriteAsync(name, value).ConfigureAwait(false);
            }
        }

        public async Task WriteAsync<T>(string name, IReadOnlyDictionary<string, T> values) where T : IFormattable
        {
            await _stream.WriteNetworkAsync(values.Count).ConfigureAwait(false);
            foreach(var entry in values) {
                await _stream.WriteNetworkAsync(entry.Key).ConfigureAwait(false);
                await WriteAsync(name, entry.Value).ConfigureAwait(false);
            }
        }

        public async Task WriteAsync(string name, IFormattable value)
        {
            await StartElementAsync(value.Type).ConfigureAwait(false);
            await value.SerializeAsync(this).ConfigureAwait(false);
            await EndElementAsync().ConfigureAwait(false);
        }
#endregion

#region Read
        public async Task<string> ReadStringAsync(string name)
        {
            return await _stream.ReadNetworkStringAsync().ConfigureAwait(false);
        }

        public async Task<byte> ReadByteAsync(string name)
        {
            return (byte)await _stream.ReadByteAsync().ConfigureAwait(false);
        }

        public async Task<bool> ReadBoolAsync(string name)
        {
            return await _stream.ReadBoolAsync().ConfigureAwait(false);
        }

        public async Task<int> ReadIntAsync(string name)
        {
            return await _stream.ReadNetworkIntAsync().ConfigureAwait(false);
        }

        public async Task<long> ReadLongAsync(string name)
        {
            return await _stream.ReadNetworkLongAsync().ConfigureAwait(false);
        }

        public async Task<float> ReadFloatAsync(string name)
        {
            return await _stream.ReadNetworkFloatAsync().ConfigureAwait(false);
        }

        public async Task<double> ReadDoubleAsync(string name)
        {
            return await _stream.ReadNetworkDoubleAsync().ConfigureAwait(false);
        }

        public async Task ReadAsync(string name, byte[] value, int offset, int count)
        {
            await _stream.ReadAsync(value, offset, count).ConfigureAwait(false);
        }

        public async Task<List<T>> ReadListAsync<T>(string name) where T : IFormattable, new()
        {
            List<T> values = new List<T>();

            int count = await _stream.ReadNetworkIntAsync().ConfigureAwait(false);
            for(int i = 0; i < count; ++i) {
                values.Add(await ReadAsync<T>(name).ConfigureAwait(false));
            }

            return values;
        }

        public async Task<Dictionary<string, T>> ReadDictionaryAsync<T>(string name) where T : IFormattable, new()
        {
            Dictionary<string, T> values = new Dictionary<string, T>();

            int count = await _stream.ReadNetworkIntAsync().ConfigureAwait(false);
            for(int i = 0; i < count; ++i) {
                string key = await _stream.ReadNetworkStringAsync().ConfigureAwait(false);
                T value = await ReadAsync<T>(name).ConfigureAwait(false);
                values[key] = value;
            }

            return values;
        }

        public async Task<T> ReadAsync<T>(string name) where T : IFormattable, new()
        {
            T value = new T();
            await value.DeserializeAsync(this).ConfigureAwait(false);
            return value;
        }
#endregion
    }
}
