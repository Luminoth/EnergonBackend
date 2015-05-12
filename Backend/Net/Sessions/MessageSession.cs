using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages;
using EnergonSoftware.Backend.Messages.Packet;
using EnergonSoftware.Backend.Properties;

using EnergonSoftware.Core.Net.Sessions;

using log4net;

namespace EnergonSoftware.Backend.Net.Sessions
{
    public abstract class MessageSession : NetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageSession));

        public async Task SendMessageAsync(IMessage message)
        {
            try {
                MessagePacket packet = CreatePacket(message);
                packet.Content = message;
                Logger.Debug("Sending packet: " + packet);

                using(MemoryStream buffer = new MemoryStream()) {
                    await packet.SerializeAsync(buffer, FormatterType).ConfigureAwait(false);
                    await CopyAsync(buffer).ConfigureAwait(false);
                }
            } catch(MessageException e) {
                InternalErrorAsync(Resources.ErrorSendingMessage, e).Wait(); 
            }
        }

        protected MessageSession() : base()
        {
        }

        protected MessageSession(Socket socket) : base(socket)
        {
        }

        protected abstract MessagePacket CreatePacket(IMessage message);
    }
}
