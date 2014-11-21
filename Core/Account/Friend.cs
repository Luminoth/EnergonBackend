using System;
using System.ComponentModel;
using System.IO;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Account
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

    [Serializable]
    public sealed class Friend : IMessageSerializable
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }

        public bool Offline { get { return Status.Offline == Status; } }
        public bool Online { get { return !Offline; } }

        public Friend()
        {
            Id = -1;
            Status = Status.Offline;
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
