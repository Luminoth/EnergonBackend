using System.Data.Common;

namespace EnergonSoftware.Database.Objects
{
    public interface IDatabaseObject
    {
        bool Dirty { get; set; }
        void Clean();

        bool Read(DatabaseConnection connection);
        void Load(DbDataReader reader);
        void Insert(DatabaseConnection connection);
        void Update(DatabaseConnection connection);
        void Delete(DatabaseConnection connection);
    }
}
