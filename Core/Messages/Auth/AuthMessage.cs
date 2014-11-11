using System.IO;

using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class AuthMessage : IMessage
    {
        public const string MESSAGE_TYPE = "auth";
        public string Type { get { return MESSAGE_TYPE; } }

        public int Version { get; set; }
        public AuthType MechanismType { get; set; }

        public string Mechanism { get { return EnumDescription.GetDescriptionFromEnumValue(MechanismType); } }

        public AuthMessage()
        {
            Version = Common.AUTH_VERSION;
            MechanismType = AuthType.DigestSHA512;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteInt(Version, stream);
            formatter.WriteInt((int)MechanismType, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Version = formatter.ReadInt(stream);
            MechanismType = (AuthType)formatter.ReadInt(stream);
        }
    }
}
