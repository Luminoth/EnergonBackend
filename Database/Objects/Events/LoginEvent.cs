using System.Collections.Generic;
using System.Data.Common;

namespace EnergonSoftware.Database.Objects.Events
{
    public enum LoginEventType
    {
        Request,
        Success,
        Failure,
        Logout,
    }

    public sealed class LoginEvent : Event
    {
        private static readonly TableDescription LOGIN_EVENTS_TABLE = new TableDescription("events_login",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("timestamp", DatabaseType.DateTime).SetNotNull() },
                { new ColumnDescription("type", DatabaseType.Integer).SetNotNull() },
                { new ColumnDescription("origin", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("account", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("reason", DatabaseType.Text) },
            }
        );

        public static void CreateTable(DatabaseConnection connection)
        {
            LOGIN_EVENTS_TABLE.Create(connection);
        }

        public readonly LoginEventType Type;

        private string _account;
        public string Account { get { return _account; } set { _account = value; Dirty = true; } }

        private string _origin;
        public string Origin { get { return _origin; } set { _origin = value; Dirty = true; } }

        private string _reason;
        public string Reason { get { return _reason; } set { _reason = value; Dirty = true; } }

        public LoginEvent(LoginEventType type)
        {
            Type = type;
        }

        public override void Insert(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + LOGIN_EVENTS_TABLE.Name
                + "(timestamp, type, origin, account, reason)"
                + " VALUES(@timestamp, @type, @origin, @account, @reason)"))
            {
                connection.AddParameter(command, "timestamp", Timestamp);
                connection.AddParameter(command, "type", Type);
                connection.AddParameter(command, "origin", Origin);
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "reason", Reason);
                command.ExecuteNonQuery();
                Id = connection.LastInsertRowId;
            }
        }

        public override string ToString()
        {
            return "LoginEvent(id: " + Id + ", timestamp: " + Timestamp + ", type: " + Type + ", origin: " + Origin + ", account: " + Account + ", reason: " + Reason + ")";
        }
    }
}