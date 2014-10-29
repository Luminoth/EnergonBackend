using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Overmind
{
    public sealed class LoginMessage : IMessage
    {
        public const string MESSAGE_TYPE = "login";
        public string Type { get { return MESSAGE_TYPE; } }

        public string Username = "";
        public string Ticket = "";

        public LoginMessage()
        {
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
    }
}
