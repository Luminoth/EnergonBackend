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
                { new ColumnDescription("account", DatabaseType.Integer).SetReferences("accounts", "id") },
                { new ColumnDescription("parent", DatabaseType.Integer).SetReferences("friend_groups", "id") },
                { new ColumnDescription("name", DatabaseType.Text).SetNotNull() },
            }
        );

        public static string TableName { get { return FriendGroupsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await FriendGroupsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        public static async Task<List<FriendGroup>> ReadAllAsync(DatabaseConnection connection, long accountId)
        {
            List<FriendGroup> groups = new List<FriendGroup>();

            using(DbCommand command = connection.BuildCommand("SELECT * FROM " + FriendGroupsTable.Name + " WHERE account=@account")) {
                connection.AddParameter(command, "account", accountId);
                using(DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while(await reader.ReadAsync().ConfigureAwait(false)) {
                        FriendGroup group = new FriendGroup();
                        group.Load(reader);
                        groups.Add(group);
                    }
                }
            }

            return groups;
        }

        public static async Task DeleteAllAsync(DatabaseConnection connection, long accountId)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + FriendGroupsTable.Name + " WHERE account=@account")) {
                connection.AddParameter(command, "account", accountId);
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

        private long _account = -1;
        public long Account { get { return _account; } set { _account = value; Dirty = true; } }

        private long _parent = -1;
        public long Parent { get { return _parent; } set { _parent = value; Dirty = true; } }

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

            using(DbCommand command = connection.BuildCommand("SELECT * FROM " + FriendGroupsTable.Name + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                using(DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if(!await reader.ReadAsync().ConfigureAwait(false)) {
                        return false;
                    }
                    Load(reader);
                }
            }

            Clean();
            return true;
        }

        public void Load(DbDataReader reader)
        {
            if(null == reader) {
                throw new ArgumentNullException("reader");
            }

            Id = reader.GetInt32(FriendGroupsTable["id"].Id);
            _account = reader.GetInt32(FriendGroupsTable["account"].Id);
            _parent = reader.GetInt32(FriendGroupsTable["parent"].Id);
            _name = reader.GetString(FriendGroupsTable["name"].Id);
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + FriendGroupsTable.Name
                + "(account, parent, name) VALUES(@account, @parent, @name)"))
            {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "parent", Parent);
                connection.AddParameter(command, "name", Name);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                Id = connection.LastInsertRowId;
            }
        }

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + FriendGroupsTable.Name
                + " SET account=@account, parent=@parent, name=@name WHERE id=@id"))
            {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "parent", Parent);
                connection.AddParameter(command, "name", Name);
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            Clean();
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + FriendGroupsTable.Name + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public override string ToString()
        {
            return "FriendGroup(id: " + Id + ", account: " + Account + ", parent: " + Parent +", name: " + Name + ")";
        }
    }
}