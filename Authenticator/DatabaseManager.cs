using System.Configuration;

using EnergonSoftware.Database;

namespace EnergonSoftware.Authenticator
{
    internal static class DatabaseManager
    {
        public static DatabaseConnection AcquireDatabaseConnection()
        {
            DatabaseConnection connection = new DatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
            connection.Open();
            return connection;
        }
    }
}
