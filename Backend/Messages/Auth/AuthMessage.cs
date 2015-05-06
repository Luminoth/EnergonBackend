using System;
using System.Threading.Tasks;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Backend.Messages.Auth
{
    [Serializable]
    public sealed class AuthMessage : IMessage
    {
        public const string MessageType = "auth";
        public string Type { get { return MessageType; } }

        public int Version { get; set; }
        public AuthType MechanismType { get; set; }

        public string Mechanism { get { return EnumDescription.GetDescriptionFromEnumValue(MechanismType); } }

        public AuthMessage()
        {
            Version = Common.AuthVersion;
            MechanismType = AuthType.DigestSHA512;
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            await formatter.WriteAsync("version", Version).ConfigureAwait(false);
            await formatter.WriteAsync("mechanism", (int)MechanismType).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            Version = await formatter.ReadIntAsync("version").ConfigureAwait(false);
            MechanismType = (AuthType)await formatter.ReadIntAsync("mechanism").ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "AuthMessage(Version=" + Version + ", Mechanism=" + Mechanism + ")";
        }
    }
}
