using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    [Serializable]
    public sealed class LoginMessage : IMessage
    {
        public const string MessageType = "login";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string SessionId { get; set; }

        public LoginMessage()
        {
            Username = string.Empty;
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteStringAsync(Username, stream).ConfigureAwait(false);
            await formatter.WriteStringAsync(SessionId, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "LoginMessage(Username=" + Username + ", SessionId=" + SessionId + ")";
        }
    }
}
