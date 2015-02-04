using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Models.Events
{
    public enum AuthEventType
    {
        Invalid,
        Request,
        Begin,
        Success,
        Failure,
    }

    public sealed class AuthEvent : Event
    {
        private static readonly TableDescription AuthEventsTable = new TableDescription(
            "events_authenticate",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("timestamp", DatabaseType.DateTime).SetNotNull() },
                { new ColumnDescription("type", DatabaseType.Integer).SetNotNull() },
                { new ColumnDescription("origin", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("account", DatabaseType.Text) },
                { new ColumnDescription("reason", DatabaseType.Text) },
            }
        );

        public static string TableName { get { return AuthEventsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await AuthEventsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        public readonly AuthEventType Type;

        private string _account;
        public string Account { get { return _account; } set { _account = value; Dirty = true; } }

        private string _origin;
        public string Origin { get { return _origin; } set { _origin = value; Dirty = true; } }

        private string _reason;
        public string Reason { get { return _reason; } set { _reason = value; Dirty = true; } }

        private AuthEvent()
        {
            Type = AuthEventType.Invalid;
        }

        public AuthEvent(AuthEventType type)
        {
            Type = type;
        }

        public override async Task InsertAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + TableName
                + "(timestamp, type, origin, account, reason)"
                + " VALUES(@timestamp, @type, @origin, @account, @reason)"))
            {
                connection.AddParameter(command, "timestamp", Timestamp);
                connection.AddParameter(command, "type", Type);
                connection.AddParameter(command, "origin", Origin);
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "reason", Reason);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                Id = connection.LastInsertRowId;
            }
        }

        public override string ToString()
        {
            return "AuthEvent(id: " + Id + ", timestamp: " + Timestamp + ", type: " + Type + ", origin: " + Origin + ", account: " + Account + ", reason: " + Reason + ")";
        }
    }
}