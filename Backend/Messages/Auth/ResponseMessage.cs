using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication challenge response
    /// </summary>
    [Serializable]
    public sealed class ResponseMessage : Message
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "response";

        public override string Type => MessageType;

        /// <summary>
        /// Gets or sets the challenge response.
        /// </summary>
        /// <value>
        /// The challenge response.
        /// </value>
        public string Response { get; set; } = string.Empty;

        public async override Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Response", Response).ConfigureAwait(false);
        }

        public async override Task DeserializeAsync(IFormatter formatter)
        {
            Response = await formatter.ReadStringAsync("Response").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"ResponseMessage(Response={Response})";
        }
    }
}
