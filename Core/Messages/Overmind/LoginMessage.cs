using System;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Overmind
{
    [Serializable]
    public sealed class LoginMessage : IMessage
    {
        public const string MessageType = "login";
        public string Type { get { return MessageType; } }

        public string Username { get; set; }
        public string Ticket { get; set; }

        public LoginMessage()
        {
            Username = string.Empty;
            Ticket = string.Empty;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            formatter.WriteString(Username, stream);
            formatter.WriteString(Ticket, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            Username = formatter.ReadString(stream);
            Ticket = formatter.ReadString(stream);
        }

        public override string ToString()
        {
            return "LoginMessage(Username=" + Username + ", Ticket=" + Ticket + ")";
        }
    }
}
