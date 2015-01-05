using System.IO;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageSerializable
    {
        // these may throw MessageException on error
        Task SerializeAsync(Stream stream, IMessageFormatter formatter);
        Task DeSerializeAsync(Stream stream, IMessageFormatter formatter);
    }
}
