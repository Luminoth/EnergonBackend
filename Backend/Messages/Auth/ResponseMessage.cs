using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication challenge response
    /// </summary>
    [Serializable]
    public sealed class ResponseMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "response";

        public string Type { get { return MessageType; } }

        /// <summary>
        /// Gets or sets the challenge response.
        /// </summary>
        /// <value>
        /// The challenge response.
        /// </value>
        public string Response { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMessage"/> class.
        /// </summary>
        public ResponseMessage()
        {
            Response = string.Empty;
        }

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Response", Response).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            Response = await formatter.ReadStringAsync("Response").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "ResponseMessage(Response=" + Response + ")";
        }
    }
}
