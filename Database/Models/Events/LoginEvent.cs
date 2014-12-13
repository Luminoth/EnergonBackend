using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Models.Events
{
    public enum LoginEventType
    {
        Invalid,
        Request,
        Success,
        Failure,
        Logout,
    }

    public sealed class LoginEvent : Event
    {
        private static readonly TableDescription LoginEventsTable = new TableDescription(
            "events_login",
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

        public static async Task CreateTable(DatabaseConnection connection)
        {
            await LoginEventsTable.Create(connection);
        }

        public readonly LoginEventType Type;

        private string _account;
        public string Account { get { return _account; } set { _account = value; Dirty = true; } }

        private string _origin;
        public string Origin { get { return _origin; } set { _origin = value; Dirty = true; } }

        private string _reason;
        public string Reason { get { return _reason; } set { _reason = value; Dirty = true; } }

        private LoginEvent()
        {
            Type = LoginEventType.Invalid;
        }

        public LoginEvent(LoginEventType type)
        {
            Type = type;
        }

        public override async Task Insert(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + LoginEventsTable.Name
                + "(timestamp, type, origin, account, reason)"
                + " VALUES(@timestamp, @type, @origin, @account, @reason)"))
            {
                connection.AddParameter(command, "timestamp", Timestamp);
                connection.AddParameter(command, "type", Type);
                connection.AddParameter(command, "origin", Origin);
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "reason", Reason);
                await Task.Run(() => command.ExecuteNonQuery());
                Id = connection.LastInsertRowId;
            }
        }

        public override string ToString()
        {
            return "LoginEvent(id: " + Id + ", timestamp: " + Timestamp + ", type: " + Type + ", origin: " + Origin + ", account: " + Account + ", reason: " + Reason + ")";
        }
    }
}