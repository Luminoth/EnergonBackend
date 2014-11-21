using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class SuccessMessage : IMessage
    {
        public const string MessageType = "success";
        public string Type { get { return MessageType; } }

        public string SessionId { get; set; }

        public SuccessMessage()
        {
            SessionId = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(SessionId, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            SessionId = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "SuccessMessage(" + SessionId + ")";
        }
    }
}
