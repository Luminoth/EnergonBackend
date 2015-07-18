using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;

using log4net;

namespace EnergonSoftware.Backend.Net.Sessions
{
    public abstract class MessageSession : NetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageSession));

        protected abstract string FormatterType { get; }

        public async Task SendMessageAsync(IMessage message)
        {
            try {
                IPacket packet = CreatePacket(message);
                packet.Content = message;
                Logger.Debug("Sending packet: " + packet);

                using(MemoryStream buffer = new MemoryStream()) {
                    await packet.SerializeAsync(buffer, FormatterType).ConfigureAwait(false);
                    await SendAsync(buffer).ConfigureAwait(false);
                }
            } catch(MessageException e) {
                InternalErrorAsync(Resources.ErrorSendingMessage, e).Wait(); 
            }
        }

        protected MessageSession()
        {
        }

        protected MessageSession(Socket socket) : base(socket)
        {
        }

        protected abstract IPacket CreatePacket(IMessage message);
    }
}
