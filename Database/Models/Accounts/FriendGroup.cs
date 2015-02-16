using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Models.Accounts
{
    public sealed class FriendGroup : IDatabaseObject
    {
        private static readonly TableDescription FriendGroupsTable = new TableDescription(
            "friend_groups",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("account_id", DatabaseType.Integer).SetReferences("accounts", "id") },
                { new ColumnDescription("parent_group_id", DatabaseType.Integer).SetDefaultValue("-1").SetReferences("friend_groups", "id") },
                { new ColumnDescription("name", DatabaseType.Text).SetNotNull() },
            });

        public static string TableName { get { return FriendGroupsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await FriendGroupsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        public static async Task DeleteAllAsync(DatabaseConnection connection, long accountId)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + TableName + " WHERE account_id=@account_id")) {
                connection.AddParameter(command, "account_id", accountId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

#region Cleanliness
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        public long Id { get; private set; }

        private long _account_id = -1;
        public long AccountId { get { return _account_id; } set { _account_id = value; Dirty = true; } }

        private string _name = string.Empty;
        public string Name { get { return _name; } set { _name = value; Dirty = true; } }

        public FriendGroup()
        {
            Id = -1;
        }

        public async Task<bool> ReadAsync(DatabaseConnection connection)
        {
            if(Id < 0) {
                return false;
            }

            using(DbCommand command = connection.BuildCommand("SELECT * FROM " + TableName + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                using(DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if(!await reader.ReadAsync().ConfigureAwait(false)) {
                        return false;
                    }

                    await LoadAsync(reader).ConfigureAwait(false);
                }
            }

            Clean();
            return true;
        }

        public async Task LoadAsync(DbDataReader reader)
        {
            if(null == reader) {
                throw new ArgumentNullException("reader");
            }

            Id = reader.GetInt32(FriendGroupsTable["id"].Id);
            _account_id = reader.GetInt32(FriendGroupsTable["account_id"].Id);
            _name = reader.GetString(FriendGroupsTable["name"].Id);

            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + TableName
                + "(account_id, name) VALUES(@account_id, @name)"))
            {
                connection.AddParameter(command, "account_id", AccountId);
                connection.AddParameter(command, "name", Name);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                Id = connection.LastInsertRowId;
            }
        }

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + TableName
                + " SET account_id=@account_id, name=@name WHERE id=@id"))
            {
                connection.AddParameter(command, "account_id", AccountId);
                connection.AddParameter(command, "name", Name);
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            Clean();
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + TableName + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public override string ToString()
        {
            return "FriendGroup(id: " + Id + ", account_id: " + AccountId + ", name: " + Name + ")";
        }
    }
}