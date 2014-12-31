using System;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages
{
    [Serializable]
    public sealed class LogoutMessage : IAuthenticatedMessage
    {
        public const string MessageType = "logout";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string SessionId { get; set; }

        public LogoutMessage()
        {
            Username = string.Empty;
            SessionId = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Username, stream);
            formatter.WriteString(SessionId, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Username = formatter.ReadString(stream);
            SessionId = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "LogoutMessage()";
        }
    }
}
