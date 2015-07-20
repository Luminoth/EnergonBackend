using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Serialization
{
    /// <summary>
    /// Formatter interface.
    /// </summary>
    public interface IFormatter
    {
        /// <summary>
        /// Gets the formatter type.
        /// </summary>
        /// <value>
        /// The formatter type.
        /// </value>
        string Type { get; }

        /// <summary>
        /// Attaches the formatter to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to attach to.</param>
        void Attach(Stream stream);

        /// <summary>
        /// Flushes the formatter.
        /// </summary>
        Task FlushAsync();

        /// <summary>
        /// Call at the start of the formatting process.
        /// </summary>
        /// <param name="documentRoot">The document root.</param>
        Task BeginAsync(string documentRoot);

        /// <summary>
        /// Call at the end of the formatting process.
        /// </summary>
        /// <returns></returns>
        Task FinishAsync();

        /// <summary>
        /// Starts an element.
        /// </summary>
        /// <param name="name">The element name.</param>
        Task StartElementAsync(string name);

        /// <summary>
        /// Ends the last started element.
        /// </summary>
        Task EndElementAsync();

#region Write
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
