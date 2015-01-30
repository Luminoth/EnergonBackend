using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Launcher.Properties;

namespace EnergonSoftware.Launcher
{
    internal class FriendListManager : INotifyPropertyChanged
    {
        public static readonly FriendListManager Instance = new FriendListManager();

        private Dictionary<string, Account> _friendList = new Dictionary<string, Account>();
        public IReadOnlyDictionary<string, Account> FriendList { get { return _friendList; } }

        public int Total { get { return _friendList.Count; } }
        public int OnlineCount { get; private set; }

        public string FriendButtonText { get { return string.Format(Resources.FriendsLabel, OnlineCount, Total); } }

        public void Clear()
        {
            _friendList.Clear();
            OnlineCount = 0;
        }

        public void AddAll(IReadOnlyCollection<Account> friendList)
        {
            foreach(Account friend in friendList) {
                AddFriend(friend);
            }
        }

        public void AddFriend(Account friend)
        {
            _friendList[friend.Username] = friend;
            // TODO: update the online count
        }

        public void UpdateFriend(Account friend)
        {
            _friendList[friend.Username] = friend;
            // TODO: update the online count
        }

        public void RemoveFriend(Account friend)
        {
            _friendList.Remove(friend.Username);
            // TODO: update the online count
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
