using System;
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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("username", Username).ConfigureAwait(false);
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync("username").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "LoginMessage(Username=" + Username + ", SessionId=" + SessionId + ")";
        }
    }
}
