using System;
using System.IO;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Overmind
{
    [Serializable]
    public sealed class LogoutMessage : IMessage
    {
        public const string MessageType = "logout";
        public string Type { get { return MessageType; } }

        public LogoutMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public override string ToString()
        {
            return "LogoutMessage()";
        }
    }
}
