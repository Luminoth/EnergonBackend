using System;
using System.IO;
using System.Net;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Accounts
{
    [Serializable]
    public sealed class Account : IMessageSerializable
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string SessionId { get; set; }
        public EndPoint EndPoint { get; set; }

        public Visibility Visibility { get; set; }
        public string Status { get; set; }

        public Account()
        {
            Id = -1;
            Visibility = Visibility.Offline;
        }

        public void Serialize(Stream stream, IMessageFormatter formatter)
        {
            // Id not serialized
            formatter.WriteString(Username, stream);
            formatter.WriteInt((int)Visibility, stream);
        }

        public void DeSerialize(Stream stream, IMessageFormatter formatter)
        {
            // Id not serialized
            Username = formatter.ReadString(stream);
            Visibility = (Visibility)formatter.ReadInt(stream);
        }

        public override bool Equals(object obj)
        {
            if(null == obj) {
                return false;
            }

            Account account = obj as Account;
            if(null == account) {
                return false;
            }

            return Username == account.Username && SessionId == account.SessionId && EndPoint == account.EndPoint;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
