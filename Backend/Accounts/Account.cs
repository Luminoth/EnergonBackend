using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using EnergonSoftware.Core.Serialization;

namespace EnergonSoftware.Backend.Accounts
{
    /// <summary>
    /// Represents an account in the system.
    /// </summary>
    [Serializable]
    public sealed class Account : Core.Serialization.IFormattable, INotifyPropertyChanged
    {
        public string Type => "account";

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public long Id { get; set; } = -1;

        private string _accountName = string.Empty;

        /// <summary>
        /// Gets or sets the name of the account.
        /// </summary>
        /// <value>
        /// The name of the account.
        /// </value>
        public string AccountName
        {
            get { return _accountName; }

            set
            {
                _accountName = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the session identifier.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the connection end point.
        /// </summary>
        /// <value>
        /// The connection end point.
        /// </value>
        public EndPoint EndPoint { get; set; }

        /// <summary>
        /// Gets or sets the account visibility.
        /// </summary>
        /// <value>
        /// The account visibility.
        /// </value>
        public Visibility Visibility { get; set; } = Visibility.Offline;

        /// <summary>
        /// Gets or sets the account status.
        /// </summary>
        /// <value>
        /// The account status.
        /// </value>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the name of the group the account is in.
        /// </summary>
        /// <value>
        /// The name of the group the account is in.
        /// </value>
        public string GroupName { get; set; }

        public async Task SerializeAsync(IFormatter formatter)
        {
            // Id not serialized
            await formatter.WriteAsync("Username", Username).ConfigureAwait(false);
            await formatter.WriteAsync("Visibility", (int)Visibility).ConfigureAwait(false);
            await formatter.WriteAsync("Status", Status ?? string.Empty).ConfigureAwait(false);
            await formatter.WriteAsync("GroupName", GroupName ?? string.Empty).ConfigureAwait(false);
        }

        public async Task DeserializeAsync(IFormatter formatter)
        {
            // Id not serialized
            Username = await formatter.ReadStringAsync("Username").ConfigureAwait(false);
            Visibility = (Visibility)await formatter.ReadIntAsync("Visibility").ConfigureAwait(false);
            Status = await formatter.ReadStringAsync("Status").ConfigureAwait(false);
            GroupName = await formatter.ReadStringAsync("GroupName").ConfigureAwait(false);
        }

        public override bool Equals(object obj)
        {
            Account account = obj as Account;
            if(account == null) {
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
            return (null != EndPoint || null == account.EndPoint) && (null == EndPoint || thisEndPoint.Address.Equals(otherEndPoint.Address));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "Account(Id=" + Id + ", AccountName=" + AccountName + ", Username=" + Username + ", SessionId=" + SessionId + ", EndPoint=" + EndPoint + ", Visibility=" + Visibility + ", Status=" + Status + ", GroupName=" + GroupName + ")";
        }

#region Property Notifier
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
#endregion
    }
}
