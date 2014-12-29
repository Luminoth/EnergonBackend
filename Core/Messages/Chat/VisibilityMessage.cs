using System;
using System.IO;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Chat
{
    [Serializable]
    public sealed class VisibilityMessage : IMessage
    {
        public const string MessageType = "visibility";
        public string Type { get { return MessageType; } }

        public Visibility Visibility { get; set; }

        public VisibilityMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteInt((int)Visibility, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Visibility = (Visibility)formatter.ReadInt(stream);
        }

        public override string ToString()
        {
            return "VisibilityMessage(Visibility=" + Visibility + ")";
        }
    }
}
