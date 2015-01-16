using System;
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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("username", Username).ConfigureAwait(false);
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync("visibility", (int)Visibility).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Username = await formatter.ReadStringAsync("username").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("ticket").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("visibility").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "VisibilityMessage(Visibility=" + Visibility + ")";
        }
    }
}
