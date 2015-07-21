using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using EnergonSoftware.Backend.Accounts;
using EnergonSoftware.Launcher.Controls;
using EnergonSoftware.Launcher.Properties;

using log4net;

namespace EnergonSoftware.Launcher.Friends
{
    internal class FriendListManager : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FriendListManager));

        public static readonly FriendListManager Instance = new FriendListManager();

        private static readonly IReadOnlyCollection<Account> TestFriends = new List<Account>()
        {
            new Account { Username = "Offline Group Friend", GroupName = "Test", Visibility = Visibility.Offline, Status = string.Empty, },
            new Account { Username = "Online Group Friend", GroupName = "Test", Visibility = Visibility.Online, Status = string.Empty, },
            new Account { Username = "Offline Friend", GroupName = string.Empty, Visibility = Visibility.Offline, Status = string.Empty, },
            new Account { Username = "Online Friend", GroupName = string.Empty, Visibility = Visibility.Online, Status = string.Empty, },
        };

        private readonly Dictionary<string, Account> _friendList = new Dictionary<string, Account>();
        public IReadOnlyDictionary<string, Account> FriendList => _friendList;

        public FriendGroupEntry RootGroupEntry { get; }

        public int Total => FriendList.Count;
        public int OnlineCount { get { return FriendList.Count(e => e.Value.Visibility.IsOnline()); } }

        public string FriendButtonText => string.Format(Resources.FriendsLabel, OnlineCount, Total);

        private void UpdateOnlineCount()
        {
            NotifyPropertyChanged("Total");
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
            Logger.Debug($"Adding friend: {friend}");
            _friendList[friend.Username] = friend;

            FriendGroupEntry group = RootGroupEntry;
            if(!string.IsNullOrEmpty(friend.GroupName)) {
                try {
                    group = RootGroupEntry.Groups[friend.GroupName];
                } catch(KeyNotFoundException) {
                    group = new FriendGroupEntry() { Text = friend.GroupName };
                    RootGroupEntry.AddGroup(group);
                }
            }

            group.AddFriend(new FriendEntry() { Text = friend.Username });
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

        public void PopulateTestFriends()
        {
            AddAll(TestFriends);
        }

        public override string ToString()
        {
            return string.Join(", ", (object[])FriendList.Values.ToArray());
        }

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
#endregion

        private FriendListManager()
        {
            RootGroupEntry = new FriendGroupEntry()
            {
                Text = "Friends",
            };
        }
    }
}
