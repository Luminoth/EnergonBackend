using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace EnergonSoftware.Launcher.Controls
{
    internal sealed class FriendGroupEntry
    {
        public string Text { get; set; }

        public ObservableCollection<FriendGroupEntry> Groups { get; set; }
        public ObservableCollection<FriendEntry> Friends { get; set; }

        public IList Entries
        {
            get
            {
                return new CompositeCollection()
                {
                    new CollectionContainer() { Collection = Groups },
                    new CollectionContainer() { Collection = Friends },
                };
            }
        }
    }
}
