using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Messages.Formatter;

namespace EnergonSoftware.Backend.Messages.Auth
{
    [Serializable]
    public sealed class LogoutMessage : IAuthenticatedMessage
    {
        public const string MessageType = "logout";
        public string Type { get { return MessageType; } }

        public string AccountName { get; set; }
        public string SessionId { get; set; }

        public LogoutMessage()
        {
            AccountName = string.Empty;
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("account_name", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("account_name").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "LogoutMessage()";
        }
    }
}
