﻿using Microsoft.Data.Sqlite;
using Snacks.Entity.Core.Database;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Sqlite
{
    /// <summary>
    /// 
    /// </summary>
    public class SqliteTableBuilder : BaseTableBuilder<SqliteService>
    {
        public SqliteTableBuilder(IDbService dbService) : base(dbService)
        {
            
        }

        public override async Task CreateTableAsync<T>()
        {
            TableMapping table = GetTableMapping<T>();

            StringBuilder tableBuilder = new StringBuilder();

            tableBuilder.Append($"CREATE TABLE IF NOT EXISTS {table.Name} (");

            foreach (TableColumnMapping column in table.Columns)
            {
                tableBuilder.Append($"{column.Name} {GetColumnDataType(column)}");

                if (column.IsKey)
                {
                    tableBuilder.Append(" PRIMARY KEY");

                    if (column.IsDatabaseGenerated &&
                        column.DatabaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                    {
                        if (GetColumnDataType(column).ToUpper().Contains("INT"))
                        {
                            tableBuilder.Append(" AUTOINCREMENT");
                        }
                    }
                }
                else
                {
                    if (column.IsRequired)
                    {
                        tableBuilder.Append(" NOT NULL");
                    }
                }

                if (table.Columns.Last() != column)
                {
                    tableBuilder.Append(',');
                }
            }

            if (table.Columns.Any(x => x.IsForeignKey))
            {
                tableBuilder.Append(',');

                var foreignKeyColumns = table.Columns.Where(x => x.IsForeignKey).ToList();

                foreach (TableColumnMapping column in foreignKeyColumns)
                {
                    tableBuilder.Append($"FOREIGN KEY ({column.Name}) REFERENCES {column.ForeignTable.Name}({column.ForeignColumn.Name})");

                    if (foreignKeyColumns.Last() != column)
                    {
                        tableBuilder.Append(',');
                    }
                }
            }

            tableBuilder.Append(")");

            using SqliteConnection connection = (SqliteConnection)await _dbService.GetConnectionAsync();

            SqliteTransaction transaction = connection.BeginTransaction();

            await _dbService.ExecuteSqlAsync(tableBuilder.ToString(), null, transaction);

            foreach (IGrouping<string, TableColumnMapping> indexedColumnGroup in table.Indexes)
            {
                StringBuilder indexSqlBuilder = new StringBuilder();

                if (indexedColumnGroup.Count() > 1)
                {
                    indexSqlBuilder.Append($"CREATE INDEX IF NOT EXISTS {indexedColumnGroup.Key} ON {table.Name} (");
                    foreach (TableColumnMapping column in indexedColumnGroup)
                    {
                        indexSqlBuilder.Append($"{column.Name}");
                        if (indexedColumnGroup.Last() != column)
                        {
                            indexSqlBuilder.Append(", ");
                        }
                    }
                }
                else
                {
                    TableColumnMapping onlyColumn = indexedColumnGroup.First();
                    if (onlyColumn.IsUnique)
                    {
                        indexSqlBuilder.Append($"CREATE UNIQUE INDEX IF NOT EXISTS {indexedColumnGroup.Key} ON {table.Name} ({onlyColumn.Name})");
                    }
                    else
                    {
                        indexSqlBuilder.Append($"CREATE INDEX IF NOT EXISTS {indexedColumnGroup.Key} ON {table.Name} ({onlyColumn.Name})");
                    }
                }

                await _dbService.ExecuteSqlAsync(indexSqlBuilder.ToString(), null, transaction);
            }

            transaction.Commit();
        }

        private string GetColumnDataType(TableColumnMapping column)
        {
            if (column.ColumnAttribute?.TypeName != null)
            {
                return column.ColumnAttribute.TypeName;
            }

            Type propertyType = Nullable.GetUnderlyingType(column.Property.PropertyType) ?? column.Property.PropertyType;

            Type[] integerTypes = new Type[]
            {
                    typeof(bool),
                    typeof(byte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(sbyte),
                    typeof(ushort),
                    typeof(uint),
                    typeof(ulong)
            };

            Type[] textTypes = new Type[]
            {
                    typeof(char),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(decimal),
                    typeof(Guid),
                    typeof(string),
                    typeof(TimeSpan)
            };

            Type[] realTypes = new Type[]
            {
                    typeof(double),
                    typeof(float)
            };

            if (integerTypes.Contains(propertyType))
            {
                return "INTEGER";
            }
            else if (textTypes.Contains(propertyType))
            {
                return "TEXT";
            }
            else if (realTypes.Contains(propertyType))
            {
                return "REAL";
            }

            throw new Exception($"Invalid type {propertyType.Name}");
        }
    }
}
