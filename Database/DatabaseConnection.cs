using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Database
{
    public sealed class DatabaseConnection : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseConnection));

        public static string ParseDataSource(ConnectionStringSettings connectionSettings)
        {
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);

            DbConnectionStringBuilder builder = providerFactory.CreateConnectionStringBuilder();
            builder.ConnectionString = connectionSettings.ConnectionString;
            return (string)builder["Data Source"];
        }

        public static async Task<bool> CreateDatabaseAsync(ConnectionStringSettings connectionSettings)
        {
            string dataSource = ParseDataSource(connectionSettings);

            Logger.Info("Creating " + connectionSettings.ProviderName + " database at " + dataSource + "...");
            try {
                switch(connectionSettings.ProviderName)
                {
                case "System.Data.SQLite":
                    await Task.Run(() => SQLiteConnection.CreateFile(dataSource)).ConfigureAwait(false);
                    return true;
                }
            } catch(AggregateException e) {
                throw e.InnerException;
            }

            return false;
        }

        public static async Task<bool> TestDatabaseConnectionAsync(ConnectionStringSettings connectionSettings)
        {
            try {
                using(DatabaseConnection connection = new DatabaseConnection(connectionSettings)) {
                    await connection.OpenAsync().ConfigureAwait(false);
                    return true;
                }
            } catch(Exception e) {
                Logger.Error("Database connection test failed!", e);
                return false;
            }
        }

        public ConnectionStringSettings ConnectionSettings { get; private set; }
        public DbConnection Connection { get; private set; }

        public long LastInsertRowId
        {
            get
            {
                switch(ConnectionSettings.ProviderName)
                {
                case "System.Data.SQLite":
                    return ((SQLiteConnection)Connection).LastInsertRowId;
                }
                return -1;
            }
        }

        private DatabaseConnection()
        {
        }

        public DatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            ConnectionSettings = connectionSettings;

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(connectionSettings.ProviderName);

            Connection = providerFactory.CreateConnection();
            Connection.ConnectionString = connectionSettings.ConnectionString;
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                Connection.Dispose();
            }
        }
#endregion

        public async Task OpenAsync()
        {
            Logger.Debug("Opening " + ConnectionSettings.ProviderName + " database connection to " + ConnectionSettings.ConnectionString + "...");
            await Connection.OpenAsync().ConfigureAwait(false);
        }

        public void Close()
        {
            Logger.Debug("Closing " + ConnectionSettings.ProviderName + " database connection to " + ConnectionSettings.ConnectionString + "...");
            Connection.Close();
        }

        public DbCommand BuildCommand(string commandText)
        {
            DbCommand command = Connection.CreateCommand();
            command.CommandText = commandText;

            Logger.Debug("Built command: " + command.CommandText);
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
            Logger.Debug("Adding parameter " + name + "=" + value);

            DbParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}
