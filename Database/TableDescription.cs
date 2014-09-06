﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

using log4net;

namespace EnergonSoftware.Database
{
    public enum DatabaseType
    {
        Boolean,
        Integer,
        Real,
        Text,
        Blob,
        DateTime,
    }

    public sealed class ColumnDescription
    {
        private static string SQLiteDatabaseTypeString(DatabaseType type)
        {
            switch(type)
            {
            case DatabaseType.Boolean:
                return "BOOLEAN";
            case DatabaseType.Integer:
                return "INTEGER";
            case DatabaseType.Real:
                return "REAL";
            case DatabaseType.Text:
                return "TEXT";
            case DatabaseType.Blob:
                return "BLOB";
            case DatabaseType.DateTime:
                return "DATETIME";
            }
            return "";
        }

        private static string DatabaseTypeString(DatabaseDriver driver, DatabaseType type)
        {
            switch(driver)
            {
            case DatabaseDriver.SQLite:
                return SQLiteDatabaseTypeString(type);
            }
            return "";
        }

        private string _name;
        private DatabaseType _type;
        private bool _primaryKey = false;
        private bool _nullable = true;
        private Tuple<string, string> _references;

        public int Id;
        public TableDescription Table;
        public string Name { get { return _name; } }
        public DatabaseType Type { get { return _type; } }
        public bool PrimaryKey { get { return _primaryKey; } }
        public bool Nullable { get { return _nullable; } }

        public bool HasForeignKey { get { return null != _references; } }
        public Tuple<string, string> References { get { return _references; } }

        public ColumnDescription(string name, DatabaseType type)
        {
            _name = name;
            _type = type;
        }

        public ColumnDescription SetPrimaryKey()
        {
            _primaryKey = true;
            return this;
        }

        public ColumnDescription SetNotNull()
        {
            _nullable = false;
            return this;
        }

        public ColumnDescription SetReferences(string table, string column)
        {
            _references = new Tuple<string, string>(table, column);
            return this;
        }

        public DbDataReader SelectAll(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("SELECT " + Name + " FROM " + Table.Name)) {
                return command.ExecuteReader();
            }
        }

        public string ToString(DatabaseDriver driver)
        {
            StringBuilder builder = new StringBuilder(Name + " " + DatabaseTypeString(driver, Type));

            if(PrimaryKey || HasForeignKey || !Nullable) {
                builder.Append(" NOT NULL");
            }

            if(HasForeignKey) {
                builder.Append(" REFERENCES " + References.Item1 + "(" + References.Item2 + ")");
            }

            return builder.ToString();
        }
    }

    public sealed class TableDescription
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TableDescription));

        private string _name;
        private Dictionary<string, ColumnDescription> _columns = new Dictionary<string, ColumnDescription>();
        private List<string> _primaryKeys = new List<string>();

        public string Name { get { return _name; } }
        //public Dictionary<string, ColumnDescription> Columns { get { return _columns; } }
        public ColumnDescription this[string key] { get { return _columns[key]; } }

        public TableDescription(string name, List<ColumnDescription> columns)
        {
            _name = name;
            for(int i=0; i<columns.Count; ++i) {
                ColumnDescription column = columns[i];
                column.Id = i;
                column.Table = this;
                _columns[column.Name] = column;
            }
            _primaryKeys.AddRange(from column in _columns.Values where column.PrimaryKey select column.Name);
        }

        public void Create(DatabaseConnection connection)
        {
            _logger.Info("Creating table " + Name + "...");

            StringBuilder create = new StringBuilder("CREATE TABLE " + _name);
            create.Append("(");
            create.Append(string.Join(", ", _columns.Values.Select(c => c.ToString(connection.Driver))));
            if(_primaryKeys.Count > 0) {
                create.Append(", PRIMARY KEY(" + string.Join(", ", _primaryKeys) + ")");
            }
            create.Append(")");

            using(DbCommand command = connection.BuildCommand(create.ToString())) {
                command.ExecuteNonQuery();
            }
        }

        public DbDataReader SelectAll(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("SELECT * FROM " + Name)) {
                return command.ExecuteReader();
            }
        }
    }
}