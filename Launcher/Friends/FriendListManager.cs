using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Launcher.Properties;

using log4net;

namespace EnergonSoftware.Launcher.Friends
{
    internal class FriendListManager : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FriendListManager));

        public static readonly FriendListManager Instance = new FriendListManager();

        private Dictionary<string, Account> _friendList = new Dictionary<string, Account>();
        public IReadOnlyDictionary<string, Account> FriendList { get { return _friendList; } }

        public int Total { get { return FriendList.Count; } }
        public int OnlineCount { get { return FriendList.Where(e => e.Value.Visibility.IsOnline()).Count(); } }

        public string FriendButtonText { get { return string.Format(Resources.FriendsLabel, OnlineCount, Total); } }

        private void UpdateOnlineCount()
        {
            NotifyPropertyChanged("OnlineCount");
            NotifyPropertyChanged("FriendButtonText");
        }

        public void Clear()
        {
            _friendList.Clear();
            UpdateOnlineCount();
        }

        public void AddAll(IReadOnlyCollection<Account> friendList)
        {
            foreach(Account friend in friendList) {
                AddFriendInternal(friend);
            }

            UpdateOnlineCount();
        }

        private void AddFriendInternal(Account friend)
        {
            Logger.Debug("Adding friend: " + friend);
            _friendList[friend.Username] = friend;
        }

        public void AddFriend(Account friend)
        {
            AddFriendInternal(friend);

            UpdateOnlineCount();
        }

        public void UpdateFriend(Account friend)
        {
            _friendList[friend.Username] = friend;

            UpdateOnlineCount();
        }

        public void RemoveFriend(Account friend)
        {
            _friendList.Remove(friend.Username);

            UpdateOnlineCount();
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion

        private FriendListManager()
        {
        }
    }
}
