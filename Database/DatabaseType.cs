namespace EnergonSoftware.Database
{
    public enum DatabaseType
    {
        Boolean,
        Integer,
        Real,
        Text,
        Blob,
        DateTime,
    }

    public static class DatabaseTypeExtensions
    {
        public static string GetTypeString(this DatabaseType type, string providerName)
        {
            switch(providerName)
            {
            case "System.Data.SQLite":
                return GetSQLiteTypeString(type);
            }
            return string.Empty;
        }

        private static string GetSQLiteTypeString(this DatabaseType type)
        {
            switch(type)
            {
            case DatabaseType.Boolean:
                return "BOOLEAN";
            case DatabaseType.Integer:
                return "INTEGER";
            case DatabaseType.Real:
                return "REAL";
            case DatabaseType.Text:
                return "TEXT";
            case DatabaseType.Blob:
                return "BLOB";
            case DatabaseType.DateTime:
                return "DATETIME";
            }
            return string.Empty;
        }
    }
}
