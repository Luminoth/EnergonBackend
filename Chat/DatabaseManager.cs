using System;
using System.Configuration;
using System.Threading.Tasks;

using EnergonSoftware.Database;

namespace EnergonSoftware.Chat
{
    internal static class DatabaseManager
    {
        public static async Task<DatabaseConnection> AcquireDatabaseConnection()
        {
            DatabaseConnection connection = new DatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
            await connection.Open();
            return connection;
        }

        public static bool TestDatabaseConnection()
        {
            return DatabaseConnection.TestDatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
        }
    }
}
