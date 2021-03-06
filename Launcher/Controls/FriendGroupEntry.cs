﻿using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;

namespace EnergonSoftware.Launcher.Controls
{
    internal sealed class FriendGroupEntry
    {
        public string Text { get; set; }

        private readonly Dictionary<string, FriendGroupEntry> _groups = new Dictionary<string, FriendGroupEntry>();
        public IReadOnlyDictionary<string, FriendGroupEntry> Groups => _groups;

        private readonly Dictionary<string, FriendEntry> _friends = new Dictionary<string, FriendEntry>();
        public IReadOnlyDictionary<string, FriendEntry> Friends => _friends;

        public IList Entries => new CompositeCollection()
        {
            new CollectionContainer { Collection = Groups.Values },
            new CollectionContainer { Collection = Friends.Values },
        };

        public void AddGroup(FriendGroupEntry group)
        {
            _groups[group.Text] = group;
        }

        public void AddFriend(FriendEntry friend)
        {
            _friends[friend.Text] = friend;
        }

        public override bool Equals(object obj)
        {
            FriendGroupEntry entry = obj as FriendGroupEntry;
            if(entry == null) {
                return false;
            }

            if(null == Text) {
                return null == entry.Text;
            }

            return Text?.Equals(entry.Text, System.StringComparison.InvariantCultureIgnoreCase) ?? null == entry.Text;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
