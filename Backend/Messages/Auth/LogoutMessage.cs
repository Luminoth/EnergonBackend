using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Logout request
    /// </summary>
    [Serializable]
    public sealed class LogoutMessage : IAuthenticatedMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "logout";

        public string Type { get { return MessageType; } }

        public string AccountName { get; set; }

        public string SessionId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutMessage"/> class.
        /// </summary>
        public LogoutMessage()
        {
            AccountName = string.Empty;
            SessionId = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("AccountName", AccountName).ConfigureAwait(false);
            await formatter.WriteAsync("Ticket", SessionId).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
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
