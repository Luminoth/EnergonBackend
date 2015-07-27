using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Net.Sessions;

namespace EnergonSoftware.Backend.MessageHandlers
{
    internal class MessageProcessorItem
    {
        public MessageNetworkSession Session { get; set; }

        public Message Message { get; set; }
    }
}
