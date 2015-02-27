﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using EnergonSoftware.Core.Accounts;
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
            new Account() { UserName = "Offline Group Friend", Group = "Test", Visibility = Visibility.Offline, Status = string.Empty, },
            new Account() { UserName = "Online Group Friend", Group = "Test", Visibility = Visibility.Online, Status = string.Empty, },
            new Account() { UserName = "Offline Friend", Group = string.Empty, Visibility = Visibility.Offline, Status = string.Empty, },
            new Account() { UserName = "Online Friend", Group = string.Empty, Visibility = Visibility.Online, Status = string.Empty, },
        };

        private readonly Dictionary<string, Account> _friendList = new Dictionary<string, Account>();
        public IReadOnlyDictionary<string, Account> FriendList { get { return _friendList; } }

        public FriendGroupEntry RootGroupEntry { get; private set; }

        public int Total { get { return FriendList.Count; } }
        public int OnlineCount { get { return FriendList.Where(e => e.Value.Visibility.IsOnline()).Count(); } }

        public string FriendButtonText { get { return string.Format(Resources.FriendsLabel, OnlineCount, Total); } }

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
            Logger.Debug("Adding friend: " + friend);
            _friendList[friend.UserName] = friend;

            FriendGroupEntry group = RootGroupEntry;
            if(!string.IsNullOrEmpty(friend.Group)) {
                try {
                    group = RootGroupEntry.Groups[friend.Group];
                } catch(KeyNotFoundException) {
                    group = new FriendGroupEntry() { Text = friend.Group };
                    RootGroupEntry.AddGroup(group);
                }
            }

            group.AddFriend(new FriendEntry() { Text = friend.UserName });
        }

        public void AddFriend(Account friend)
        {
            AddFriendInternal(friend);
            UpdateOnlineCount();
        }

        public void UpdateFriend(Account friend)
        {
            _friendList[friend.UserName] = friend;
            UpdateOnlineCount();
        }

        public void RemoveFriend(Account friend)
        {
            _friendList.Remove(friend.UserName);
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
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
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