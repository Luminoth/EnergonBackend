using System;
using System.IO;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    [Serializable]
    public sealed class VisibilityMessage : IAuthenticatedMessage
    {
        public const string MessageType = "visibility";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string SessionId { get; set; }

        public Visibility Visibility { get; set; }

        public VisibilityMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Username, stream);
            formatter.WriteString(SessionId, stream);
            formatter.WriteInt((int)Visibility, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Username = formatter.ReadString(stream);
            SessionId = formatter.ReadString(stream);
            Visibility = (Visibility)formatter.ReadInt(stream);
        }

        public override string ToString()
        {
            return "VisibilityMessage(Visibility=" + Visibility + ")";
        }
    }
}
