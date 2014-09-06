using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;

using log4net;

namespace EnergonSoftware.Database
{
    public enum DatabaseDriver
    {
        None,
        SQLite,
    }

    public class DatabaseConnection : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DatabaseConnection));

        public static void CreateDatabase(DatabaseDriver driver, string url)
        {
            _logger.Info("Creating " + driver + " database at " + url + "...");
            switch(driver)
            {
            case DatabaseDriver.SQLite:
                SQLiteConnection.CreateFile(url);
                break;
            }
        }

        private object _lock = new object();

        private DatabaseDriver _driver = DatabaseDriver.None;
        private string _url = "";
        private int _maxPoolSize = 10;

        private DbConnection _connection;

        public DatabaseDriver Driver { get { return _driver; } }
        public string Url { get { return _url; } }
        public int MaxPoolSize { get { return _maxPoolSize; } }

        public DbConnection Connection { get { return _connection; } }

        public long LastInsertRowId
        {
            get
            {
                lock(_lock) {
                    switch(Driver)
                    {
                    case DatabaseDriver.SQLite:
                        return ((SQLiteConnection)_connection).LastInsertRowId;
                    }
                    return -1;
                }
            }
        }

        public DatabaseConnection(DatabaseDriver driver, string url, int maxPoolSize)
        {
            _driver = driver;
            _url = url;
            _maxPoolSize = maxPoolSize;

            switch(Driver)
            {
            case DatabaseDriver.SQLite:
                _connection = new SQLiteConnection("Data Source=" + Url + ";Pooling=True;Max Pool Size=" + MaxPoolSize);
                break;
            default:
                throw new Exception("Unsupported database driver: " + Driver);
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public void Open()
        {
            lock(_lock) {
                _logger.Debug("Opening " + Driver + " database connection to " + Url + "...");
                _connection.Open();
            }
        }

        public void Close()
        {
            lock(_lock) {
                _logger.Debug("Closing " + Driver + " database connection to " + Url + "...");
                _connection.Close();
            }
        }

        public DbCommand BuildCommand(string commandText)
        {
            DbCommand command = _connection.CreateCommand();
            command.CommandText = commandText;

            _logger.Debug("Built command: " + command.CommandText);
            return command;
        }

        public DbCommand BuildCommand(string commandText, Dictionary<string, object> parameters)
        {
            DbCommand command = BuildCommand(commandText);
            foreach(var param in parameters) {
                AddParameter(command, param.Key, param.Value);
            }
            return command;
        }

        public void AddParameter(DbCommand command, string name, object value)
        {
            _logger.Debug("Adding parameter " + name + "=" + value);

            DbParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}
