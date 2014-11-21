using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class FailureMessage : IMessage
    {
        public const string MessageType = "failure";
        public string Type { get { return MessageType; } }

        public string Reason { get; set; }

        public FailureMessage()
        {
            Reason = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Reason, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Reason = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "FailureMessage(Reason=" + Reason + ")";
        }
    }
}
