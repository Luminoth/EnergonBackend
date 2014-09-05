using System;
using System.IO;

using EnergonSoftware.Core;
using EnergonSoftware.Database;

namespace EnergonSoftware.DbInit
{
    [Serializable]
    sealed class Configuration
    {
#region Singleton
        private static Configuration _instance = new Configuration();
        public static Configuration Instance { get { return _instance; } }
#endregion

#region Database Properties
        private DatabaseDriver _accountDatabaseDriver = DatabaseDriver.SQLite;
        private string _accountDatabaseFilename = "accounts.db";
        private int _accountDatabasePoolSize = 10;

        public DatabaseDriver AccountDatabaseDriver { get { return _accountDatabaseDriver; } }
        public string AccountDatabaseFilename { get { return Path.Combine(Common.DataDir, _accountDatabaseFilename); } }
        public int AccountDatabasePoolSize { get { return _accountDatabasePoolSize; } }
#endregion

        private Configuration()
        {
        }
    }
}
