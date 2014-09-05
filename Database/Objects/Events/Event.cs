using System.Data.Common;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Database.Objects.Events
{
    public abstract class Event : IDatabaseObject
    {
#region Cleanliness
        protected bool _dirty = false;

        public bool Dirty { get { return _dirty; } set { _dirty = value; } }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        protected long _id = -1;
        private long _timestamp = Time.CurrentTimeMs;

        public long Id { get { return _id; } }
        public long Timestamp { get { return _timestamp; } }

        public Event()
        {
        }

        public bool Read(DatabaseConnection connection)
        {
            return false;
        }

        public void Load(DbDataReader reader)
        {
        }

        public abstract void Insert(DatabaseConnection connection);

        public void Update(DatabaseConnection connection)
        {
        }

        public void Delete(DatabaseConnection connection)
        {
        }
    }
}
