using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Auth
{
    public sealed class SuccessMessage : IMessage
    {
        public string Type { get { return "success"; } }

        public string SessionId = "";

        public SuccessMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(SessionId, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            SessionId = formatter.ReadString(stream);
        }
    }
}
