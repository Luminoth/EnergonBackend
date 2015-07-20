using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Serialization.Formatters
{
    /// <summary>
    /// Formats objects in a JSON format.
    /// </summary>
    public class JsonFormatter : IFormatter
    {
        /// <summary>
        /// The formatter type
        /// </summary>
        public const string FormatterType = "JSON";

        public string Type { get { return FormatterType; } }

        private Stream _stream;

        public void Attach(Stream stream)
        {
            _stream = stream;
        }

        public Task FlushAsync()
        {
            throw new NotImplementedException();
        }

        public Task BeginAsync(string documentRoot)
        {
            throw new NotImplementedException();
        }

        public Task FinishAsync()
        {
            throw new NotImplementedException();
        }

        public Task StartElementAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task EndElementAsync()
        {
            throw new NotImplementedException();
        }

#region Write
        public Task WriteAsync(string name, string value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, byte value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, bool value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, int value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, long value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, float value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, double value)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IFormattable
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync<T>(string name, IReadOnlyDictionary<string, T> values) where T : IFormattable
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string name, IFormattable value)
        {
            throw new NotImplementedException();
        }
#endregion

#region Read
        public Task<string> ReadStringAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<byte> ReadByteAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ReadBoolAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadIntAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<long> ReadLongAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<float> ReadFloatAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<double> ReadDoubleAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task ReadAsync(string name, byte[] value, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> ReadListAsync<T>(string name) where T : IFormattable, new()
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, T>> ReadDictionaryAsync<T>(string name) where T : IFormattable, new()
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadAsync<T>(string name) where T : IFormattable, new()
        {
            throw new NotImplementedException();
        }
#endregion
    }
}