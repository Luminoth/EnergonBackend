using System;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Auth
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

        public async Task SerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            await formatter.WriteIntAsync(Version, stream).ConfigureAwait(false);
            await formatter.WriteIntAsync((int)MechanismType, stream).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(Stream stream, IMessageFormatter formatter)
        {
            Version = await formatter.ReadIntAsync(stream).ConfigureAwait(false);
            MechanismType = (AuthType)await formatter.ReadIntAsync(stream).ConfigureAwait(false);
        }

        public override string ToString()
        {
            return "AuthMessage(Version=" + Version + ", Mechanism=" + Mechanism + ")";
        }
    }
}
