using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Serialization
{
    public interface IFormatter
    {
        string Type { get; }

        void Attach(Stream stream);

#region Write
        Task FlushAsync();

        Task BeginAsync(string documentRoot);
        Task FinishAsync();

        Task StartElementAsync(string name);
        Task EndElementAsync();

        Task WriteAsync(string name, string value);
        Task WriteAsync(string name, byte value);
        Task WriteAsync(string name, bool value);
        Task WriteAsync(string name, int value);
        Task WriteAsync(string name, long value);
        Task WriteAsync(string name, float value);
        Task WriteAsync(string name, double value);

        Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IFormattable;
        Task WriteAsync<T>(string name, IReadOnlyDictionary<string, T> values) where T : IFormattable;
        Task WriteAsync(string name, IFormattable value);
#endregion

#region Read
        Task<string> ReadStringAsync(string name);
        Task<byte> ReadByteAsync(string name);
        Task<bool> ReadBoolAsync(string name);
        Task<int> ReadIntAsync(string name);
        Task<long> ReadLongAsync(string name);
        Task<float> ReadFloatAsync(string name);
        Task<double> ReadDoubleAsync(string name);

        Task ReadAsync(string name, byte[] value, int offset, int count);

        Task<List<T>> ReadListAsync<T>(string name) where T : IFormattable, new();
        Task<Dictionary<string, T>> ReadDictionaryAsync<T>(string name) where T : IFormattable, new();
        Task<T> ReadAsync<T>(string name) where T : IFormattable, new();
#endregion
    }
}
