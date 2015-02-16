using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Database
{
    public sealed class TableDescription
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TableDescription));

        private readonly Dictionary<string, ColumnDescription> _columns = new Dictionary<string, ColumnDescription>();
        private readonly List<string> _primaryKeys = new List<string>();

        public readonly string Name;
        public ColumnDescription this[string key] { get { return _columns[key]; } }

        private TableDescription()
        {
        }

        public TableDescription(string name, List<ColumnDescription> columns)
        {
            if(null == columns) {
                throw new ArgumentNullException("columns");
            }

            Name = name;
            for(int i = 0; i < columns.Count; ++i) {
                ColumnDescription column = columns[i];
                column.Id = i;
                column.Table = this;
                _columns[column.Name] = column;
            }

            _primaryKeys.AddRange(from column in _columns.Values where column.PrimaryKey select column.Name);
        }

        public async Task CreateAsync(DatabaseConnection connection)
        {
            Logger.Info("Creating table " + Name + "...");

            StringBuilder create = new StringBuilder("CREATE TABLE " + Name);
            create.Append("(");
            create.Append(string.Join(", ", _columns.Values.Select(c => c.ToString(connection.ConnectionSettings.ProviderName))));
            if(_primaryKeys.Count > 0) {
                create.Append(", PRIMARY KEY(" + string.Join(", ", _primaryKeys) + ")");
            }

            create.Append(")");

            using(DbCommand command = connection.BuildCommand(create.ToString())) {
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task<DbDataReader> SelectAllAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("SELECT * FROM " + Name)) {
                return await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
        }
    }
}