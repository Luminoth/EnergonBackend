using System.IO;

namespace EnergonSoftware.Core.Messages.Formatter
{
    public interface IMessageSerializable
    {
        // these may throw MessageException on error
        void Serialize(Stream stream, IMessageFormatter formatter);
        void DeSerialize(Stream stream, IMessageFormatter formatter);
    }
}
