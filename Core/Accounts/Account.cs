using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages.Formatter;

namespace EnergonSoftware.Core.Accounts
{
    [Serializable]
    public sealed class Account : IMessageSerializable, INotifyPropertyChanged
    {
        public string Type { get { return "account"; } }

        public long Id { get; set; }

        private string _accountName = string.Empty;
        public string AccountName
        {
            get { return _accountName; }

            set
            {
                _accountName = value;
                NotifyPropertyChanged();
            }
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string SessionId { get; set; }
        public EndPoint EndPoint { get; set; }

        public Visibility Visibility { get; set; }
        public string Status { get; set; }

        public string Group { get; set; }

        public Account()
        {
            Id = -1;
            Visibility = Visibility.Offline;
        }

        public async Task SerializeAsync(IMessageFormatter formatter)
        {
            // Id not serialized
            await formatter.WriteAsync("username", UserName).ConfigureAwait(false);
            await formatter.WriteAsync("visibility", (int)Visibility).ConfigureAwait(false);
            await formatter.WriteAsync("status", Status ?? string.Empty).ConfigureAwait(false);
            await formatter.WriteAsync("group", Group ?? string.Empty).ConfigureAwait(false);
        }

        public async Task DeSerializeAsync(IMessageFormatter formatter)
        {
            // Id not serialized
            UserName = await formatter.ReadStringAsync("username").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("visibility").ConfigureAwait(false);
            Status = await formatter.ReadStringAsync("status").ConfigureAwait(false);
            Group = await formatter.ReadStringAsync("group").ConfigureAwait(false);
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

            if((null == AccountName && null != account.AccountName) || (null != AccountName && !AccountName.Equals(account.AccountName, StringComparison.InvariantCultureIgnoreCase))) {
                return false;
            }

            if((null == SessionId && null != account.SessionId) || (null != SessionId && !SessionId.Equals(account.SessionId, StringComparison.InvariantCultureIgnoreCase))) {
                return false;
            }

            IPEndPoint thisEndPoint = (IPEndPoint)EndPoint;
            IPEndPoint otherEndPoint = (IPEndPoint)account.EndPoint;

            if((null == EndPoint && null != account.EndPoint) || (null != EndPoint && !thisEndPoint.Address.Equals(otherEndPoint.Address))) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Account(Id=" + Id + ", AccountName=" + AccountName + ", UserName=" + UserName + ", SessionId=" + SessionId + ", EndPoint=" + EndPoint + ", Visibility=" + Visibility + ", Status=" + Status + ", Group=" + Group + ")";
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string property = null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion
    }
}
