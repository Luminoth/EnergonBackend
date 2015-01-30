using System;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Accounts
{
    [Serializable]
    public sealed class Account : IMessageSerializable
    {
        public string Type { get { return "account"; } }

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

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            // Id not serialized
            await formatter.WriteAsync("username", Username).ConfigureAwait(false);
            await formatter.WriteAsync("visibility", (int)Visibility).ConfigureAwait(false);
            await formatter.WriteAsync("status", Status??string.Empty).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            // Id not serialized
            Username = await formatter.ReadStringAsync("username").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("visibility").ConfigureAwait(false);
            Status = await formatter.ReadStringAsync("status").ConfigureAwait(false);
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

            IPEndPoint thisEndPoint = (IPEndPoint)EndPoint;
            IPEndPoint otherEndPoint = (IPEndPoint)account.EndPoint;

            return Username.Equals(account.Username, StringComparison.InvariantCultureIgnoreCase)
                && SessionId.Equals(account.SessionId, StringComparison.InvariantCultureIgnoreCase)
                && thisEndPoint.Address.Equals(otherEndPoint.Address);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Account(Id=" + Id + ", Username=" + Username + ", SessionId=" + SessionId + ", EndPoint=" + EndPoint + ", Visibility=" + Visibility + ", Status=" + Status + ")";
        }
    }
}
