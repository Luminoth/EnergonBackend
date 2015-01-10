using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageFormatter
    {
        Task WriteListAsync<T>(List<T> value, Stream stream) where T : IMessageSerializable;

        Task WriteStringAsync(string value, Stream stream);
        Task WriteByteAsync(byte value, Stream stream);
        Task WriteBoolAsync(bool value, Stream stream);
        Task WriteIntAsync(int value, Stream stream);
        Task WriteLongAsync(long value, Stream stream);
        Task WriteFloatAsync(float value, Stream stream);
        Task WriteDoubleAsync(double value, Stream stream);
        Task WriteAsync(byte[] value, int offset, int count, Stream stream);

        Task<List<T>> ReadListAsync<T>(Stream stream) where T : IMessageSerializable, new();

        Task<string> ReadStringAsync(Stream stream);
        Task<byte> ReadByteAsync(Stream stream);
        Task<bool> ReadBoolAsync(Stream stream);
        Task<int> ReadIntAsync(Stream stream);
        Task<long> ReadLongAsync(Stream stream);
        Task<float> ReadFloatAsync(Stream stream);
        Task<double> ReadDoubleAsync(Stream stream);
        Task ReadAsync(byte[] value, int offset, int count, Stream stream);
    }
}
