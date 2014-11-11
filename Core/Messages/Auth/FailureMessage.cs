using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class FailureMessage : IMessage
    {
        public const string MESSAGE_TYPE = "failure";
        public string Type { get { return MESSAGE_TYPE; } }

        public string Reason { get; set; }

        public FailureMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Reason, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Reason = formatter.ReadString(stream);
        }
    }
}
