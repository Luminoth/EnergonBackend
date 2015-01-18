using System.Collections.Generic;

using EnergonSoftware.Core.Accounts;

namespace EnergonSoftware.Launcher
{
    internal class FriendListManager
    {
        public static readonly FriendListManager Instance = new FriendListManager();

        private Dictionary<string, Account> _friendList = new Dictionary<string, Account>();
        public int Total { get { return _friendList.Count; } }
        public int OnlineCount { get; private set; }

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

        private FriendListManager()
        {
        }
    }
}
