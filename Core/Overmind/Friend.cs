using System.ComponentModel;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Overmind
{
    public enum Status
    {
        [Description("Offline")]
        Offline,

        [Description("Online")]
        Online,

        [Description("Away")]
        Away,
    }

    public sealed class Friend : IMessageSerializable
    {
        public long Id = -1;
        public string Name = "";
        public Status Status = Status.Offline;

        public bool Offline { get { return Status.Offline == Status; } }
        public bool Online { get { return !Offline; } }

        public Friend()
        {
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            // Id not serialized
            formatter.WriteString(Name, stream);
            formatter.WriteInt((int)Status, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            // Id not serialized
            Name = formatter.ReadString(stream);
            Status = (Status)formatter.ReadInt(stream);
        }
    }
}
