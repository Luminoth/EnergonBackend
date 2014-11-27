using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Objects
{
    public interface IDatabaseObject
    {
        bool Dirty { get; set; }
        void Clean();

        Task<bool> Read(DatabaseConnection connection);
        void Load(DbDataReader reader);

        Task Insert(DatabaseConnection connection);
        Task Update(DatabaseConnection connection);
        Task Delete(DatabaseConnection connection);
    }
}
