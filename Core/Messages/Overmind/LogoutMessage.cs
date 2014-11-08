using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Messages.Overmind
{
    public sealed class LogoutMessage : IMessage
    {
        public const string MESSAGE_TYPE = "logout";
        public string Type { get { return MESSAGE_TYPE; } }

        public LogoutMessage()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
        }
    }
}
