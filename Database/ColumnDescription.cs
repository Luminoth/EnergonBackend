using System;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace EnergonSoftware.Database
{
    public sealed class ColumnDescription
    {
        public int Id { get; set; }
        public TableDescription Table { get; set; }

        public readonly string Name;
        public readonly DatabaseType Type;

        public bool PrimaryKey { get; private set; }
        public bool Unique { get; private set; }
        public bool Nullable { get; private set; }
        public string DefaultValue { get; private set; }

        private Tuple<string, string> _references;
        public Tuple<string, string> References { get { return _references; } }
        public bool HasForeignKey { get { return null != _references; } }

        private ColumnDescription()
        {
        }

        public ColumnDescription(string name, DatabaseType type)
        {
            Name = name;
            Type = type;
            PrimaryKey = false;
            Unique = false;
            Nullable = true;
        }

        public ColumnDescription SetPrimaryKey()
        {
            PrimaryKey = true;
            return this;
        }

        public ColumnDescription SetUnique()
        {
            Unique = true;
            return this;
        }

        public ColumnDescription SetNotNull()
        {
            Nullable = false;
            return this;
        }

        public ColumnDescription SetDefaultValue(string defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        public ColumnDescription SetReferences(string table, string column)
        {
            _references = new Tuple<string, string>(table, column);
            return this;
        }

        public async Task<DbDataReader> SelectAllAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("SELECT " + Name + " FROM " + Table.Name)) {
                return await command.ExecuteReaderAsync().ConfigureAwait(false);
            }
        }

        public string ToString(string providerName)
        {
            StringBuilder builder = new StringBuilder(Name + " " + Type.GetTypeString(providerName));

            if(PrimaryKey || HasForeignKey || !Nullable) {
                builder.Append(" NOT NULL");
            }

            if(Unique) {
                builder.Append(" UNIQUE");
            }

            if(!string.IsNullOrEmpty(DefaultValue)) {
                builder.Append(" DEFAULT " + DefaultValue);
            }

            if(HasForeignKey) {
                builder.Append(" REFERENCES " + References.Item1 + "(" + References.Item2 + ")");
            }

            return builder.ToString();
        }
    }
}
