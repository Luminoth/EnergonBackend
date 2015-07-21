using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Backend.Messages.Auth
{
    /// <summary>
    /// Authentication request
    /// </summary>
    [Serializable]
    public sealed class AuthMessage : IMessage
    {
        /// <summary>
        /// The message type
        /// </summary>
        public const string MessageType = "auth";

        public string Type => MessageType;

        /// <summary>
        /// Gets or sets the authentication protocl version.
        /// </summary>
        /// <value>
        /// The protocol version.
        /// </value>
        public int ProtocolVersion { get; set; } = Common.AuthProtocolVersion;

        /// <summary>
        /// Gets or sets the type of the authentication mechanism.
        /// </summary>
        /// <value>
        /// The type of the authentication mechanism.
        /// </value>
        public AuthType MechanismType { get; set; } = AuthType.DigestSHA512;

        /// <summary>
        /// Gets the authentication mechanism as a string.
        /// </summary>
        /// <value>
        /// The authentication mechanism as a string.
        /// </value>
        public string Mechanism => EnumDescription.GetDescriptionFromEnumValue(MechanismType);

        public async Task SerializeAsync(IFormatter formatter)
        {
            await formatter.WriteAsync("Version", ProtocolVersion).ConfigureAwait(false);
            await formatter.WriteAsync("Mechanism", (int)MechanismType).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            ProtocolVersion = await formatter.ReadIntAsync("Version").ConfigureAwait(false);
            MechanismType = (AuthType)await formatter.ReadIntAsync("Mechanism").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return $"AuthMessage(Version={ProtocolVersion}, Mechanism={Mechanism})";
        }
    }
}
