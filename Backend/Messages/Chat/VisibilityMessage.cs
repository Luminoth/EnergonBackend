using System;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Chat
{
    [Serializable]
    public sealed class VisibilityMessage : IAuthenticatedMessage
    {
        public const string MessageType = "visibility";
        public string Type { get { return MessageType; } }

        public string AccountName { get; set; }
        public string SessionId { get; set; }

        public Visibility Visibility { get; set; }

        public VisibilityMessage()
        {
            AccountName = string.Empty;
            SessionId = string.Empty;

            Visibility = Accounts.Visibility.Online;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
            await formatter.WriteAsync("Visibility", (int)Visibility).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("AccountName").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("Visibility").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "VisibilityMessage(Visibility=" + Visibility + ")";
        }
    }
}
