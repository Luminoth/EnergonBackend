using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;

using log4net;

namespace EnergonSoftware.Database
{
    public class DatabaseConnection : IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DatabaseConnection));

        public static string ParseDataSource(ConnectionStringSettings connectionSettings)
        {
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);

            DbConnectionStringBuilder builder = providerFactory.CreateConnectionStringBuilder();
            builder.ConnectionString = connectionSettings.ConnectionString;
            return (string)builder["Data Source"];
        }

        public static bool CreateDatabase(ConnectionStringSettings connectionSettings)
        {
            string dataSource = ParseDataSource(connectionSettings);

            _logger.Info("Creating " + connectionSettings.ProviderName + " database at " + dataSource + "...");
            switch(connectionSettings.ProviderName)
            {
            case "System.Data.SQLite":
                SQLiteConnection.CreateFile(dataSource);
                return true;
            }

            return false;
        }

        private object _lock = new object();

        private ConnectionStringSettings _connectionSettings;
        private DbConnection _connection;

        public ConnectionStringSettings ConnectionSettings { get { return _connectionSettings; } }
        public DbConnection Connection { get { return _connection; } }

        public long LastInsertRowId
        {
            get
            {
                lock(_lock) {
                    switch(ConnectionSettings.ProviderName)
                    {
                    case "System.Data.SQLite":
                        return ((SQLiteConnection)_connection).LastInsertRowId;
                    }
                    return -1;
                }
            }
        }

        public DatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            _connectionSettings = connectionSettings;

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);

            _connection = providerFactory.CreateConnection();
            _connection.ConnectionString = connectionSettings.ConnectionString;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public void Open()
        {
            lock(_lock) {
                _logger.Debug("Opening " + ConnectionSettings.ProviderName + " database connection to " + ConnectionSettings.ConnectionString + "...");
                _connection.Open();
            }
        }

        public void Close()
        {
            lock(_lock) {
                _logger.Debug("Closing " + ConnectionSettings.ProviderName + " database connection to " + ConnectionSettings.ConnectionString + "...");
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
