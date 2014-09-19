using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class FailureMessage : IMessage
    {
        public string Type { get { return "failure"; } }

        public string Reason = "";

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
