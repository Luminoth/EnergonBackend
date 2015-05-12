using System.Threading.Tasks;

namespace EnergonSoftware.Backend.Messages.Parser
{
    public interface IMessagePacketParser
    {
        // NOTE: this locks the stream, so do not lock before calling
        Task ParseAsync(IMessageFactory messageFactory);
    }
}
