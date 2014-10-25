using System.Data.Common;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Database.Objects.Events
{
    public abstract class Event : IDatabaseObject
    {
#region Cleanliness
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        public long Id { get; protected set; }
        public long Timestamp { get; protected set; }

        public Event()
        {
            Timestamp = Time.CurrentTimeMs;
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
