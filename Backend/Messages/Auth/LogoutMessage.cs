using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Logout request
    /// </summary>
    [Serializable]
    public sealed class LogoutMessage : AuthenticatedMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "logout";

        public override string Type => MessageType;

        public override string AccountName { get; set; } = string.Empty;

        public override string SessionId { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            AccountName = await formatter.ReadStringAsync("AccountName").ConfigureAwait(false);
            SessionId = await formatter.ReadStringAsync("Ticket").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "LogoutMessage()";
        }
    }
}
