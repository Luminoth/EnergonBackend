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
    /// <summary>
    /// Extends the NetworkSession to work with messages
    /// </summary>
    public abstract class MessageSession : NetworkSession
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessageSession));

        /// <summary>
        /// Gets the type of the formatter to use.
        /// </summary>
        /// <value>
        /// The type of the formatter to use.
        /// </value>
        protected abstract string FormatterType { get; }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
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

        /// <summary>
        /// Creates a packet from a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The packet</returns>
        protected abstract IPacket CreatePacket(IMessage message);
    }
}
