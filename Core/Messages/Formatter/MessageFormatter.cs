using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageFormatter
    {
        string Type { get; }

        // attaches the formatter to a stream
        void Attach(Stream stream);

#region Writing
        // flushes the formatter
        Task FlushAsync();

        Task StartDocumentAsync();
        Task EndDocumentAsync();

        Task StartElementAsync(string name);
        Task EndElementAsync();

        Task WriteAsync<T>(string name, IReadOnlyCollection<T> values) where T : IMessageSerializable;
        Task WriteAsync(IMessageSerializable value);
        Task WriteAsync(string name, string value);
        Task WriteAsync(string name, byte value);
        Task WriteAsync(string name, bool value);
        Task WriteAsync(string name, int value);
        Task WriteAsync(string name, long value);
        Task WriteAsync(string name, float value);
        Task WriteAsync(string name, double value);

        Task WriteAsync(byte[] value, int offset, int count);
#endregion

#region Reading
        Task<List<T>> ReadListAsync<T>(string name) where T : IMessageSerializable, new();
        Task<T> ReadAsync<T>(string name) where T : IMessageSerializable, new();

        Task<string> ReadStringAsync(string name);
        Task<byte> ReadByteAsync(string name);
        Task<bool> ReadBoolAsync(string name);
        Task<int> ReadIntAsync(string name);
        Task<long> ReadLongAsync(string name);
        Task<float> ReadFloatAsync(string name);
        Task<double> ReadDoubleAsync(string name);

        Task ReadAsync(byte[] value, int offset, int count);
#endregion
    }

    public static class MessageFormatterFactory
    {
        public static IMessageFormatter Create(string type)
        {
            switch(type)
            {
            case BinaryMessageFormatter.FormatterType:
                return new BinaryMessageFormatter();
            case XmlMessageFormatter.FormatterType:
                return new XmlMessageFormatter();
            }
            throw new MessageFormatterException(string.Format(Resources.ErrorUnsupportedMessageFormatter, type));
        }
    }
}
