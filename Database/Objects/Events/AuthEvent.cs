using System.Collections.Generic;
using System.Data.Common;

namespace EnergonSoftware.Database.Objects.Events
{
    public enum AuthEventType
    {
        Request,
        Begin,
        Success,
        Failure,
    }

    public sealed class AuthEvent : Event
    {
        private static TableDescription AUTH_EVENTS_TABLE = new TableDescription("events_authenticate",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("timestamp", DatabaseType.DateTime).SetNotNull() },
                { new ColumnDescription("type", DatabaseType.Integer).SetNotNull() },
                { new ColumnDescription("origin", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("account", DatabaseType.Text) },
            }
        );

        public static void CreateTable(DatabaseConnection connection)
        {
            AUTH_EVENTS_TABLE.Create(connection);
        }

        private AuthEventType _type;
        private string _account;
        private string _origin;

        public AuthEventType Type { get { return _type; } }
        public string Account { get { return _account; } set { _account = value; _dirty = true; } }
        public string Origin { get { return _origin; } set { _origin = value; _dirty = true; } }

        public AuthEvent(AuthEventType type)
        {
            _type = type;
        }

        public override void Insert(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AUTH_EVENTS_TABLE.Name
                + "(timestamp, type, origin, account)"
                + " VALUES(@timestamp, @type, @origin, @account)"))
            {
                connection.AddParameter(command, "timestamp", Timestamp);
                connection.AddParameter(command, "type", Type);
                connection.AddParameter(command, "origin", Origin);
                connection.AddParameter(command, "account", Account);
                command.ExecuteNonQuery();
                _id = connection.LastInsertRowId;
            }
        }

        public override string ToString()
        {
            return "AuthEvent(id: " + Id + ", timestamp: " + Timestamp + ", type: " + Type + ", origin: " + Origin + ", account: " + Account + ")";
        }
    }
}