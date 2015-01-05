using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    [Serializable]
    public sealed class VisibilityMessage : IAuthenticatedMessage
    {
        public const string MessageType = "visibility";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string SessionId { get; set; }

        public Visibility Visibility { get; set; }

        public VisibilityMessage()
        {
            Username = string.Empty;
            SessionId = string.Empty;
            Visibility = Accounts.Visibility.Online;
        }

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Username, stream).ConfigureAwait(false);
            await formatter.WriteStringAsync(SessionId, stream).ConfigureAwait(false);
            await formatter.WriteIntAsync((int)Visibility, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "VisibilityMessage(Visibility=" + Visibility + ")";
        }
    }
}
