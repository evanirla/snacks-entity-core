using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Snacks.Entity.Core
{
    public class SqliteTableBuilder : IDbTableBuilder<SqliteService, SqliteConnection>
    {
        public void Table<T>()
        {
            Type modelType = typeof(T);

            TableAttribute tableAttribute = modelType.GetCustomAttribute<TableAttribute>();

            StringBuilder tableBuilder = new StringBuilder();

            tableBuilder.Append($"CREATE TABLE IF NOT EXISTS {tableAttribute.Name} (");

            List<PropertyInfo> mappedProperties = modelType.GetProperties()
                .Where(x => !x.IsDefined(typeof(NotMappedAttribute))).ToList();
            foreach (PropertyInfo property in mappedProperties)
            {
                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                KeyAttribute keyAttribute = property.GetCustomAttribute<KeyAttribute>();
                RequiredAttribute requiredAttribute = property.GetCustomAttribute<RequiredAttribute>();

                string columnName = columnAttribute.Name ?? property.Name;
                string columnType = columnAttribute.TypeName ?? "TEXT";

                tableBuilder.Append($"{columnName} {columnType}");

                if (keyAttribute != null)
                {
                    tableBuilder.Append(" PRIMARY KEY");
                }

                if (requiredAttribute != null)
                {
                    tableBuilder.Append(" NOT NULL");
                }

                if (mappedProperties.Last() != property)
                {
                    tableBuilder.Append(',');
                }
            }

            tableBuilder.Append(")");

            List<string> indexSql = new List<string>();

            foreach (PropertyInfo property in mappedProperties)
            {
                IndexAttribute indexAttribute = property.GetCustomAttribute<IndexAttribute>();

                if (indexAttribute != null)
                {
                    ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                    string columnName = columnAttribute.Name ?? property.Name;
                    string indexName = indexAttribute.Name ?? columnAttribute.Name ?? property.Name;

                    if (indexAttribute.IsUnique)
                    {
                        indexSql.Add($"CREATE UNIQUE INDEX IF NOT EXISTS IX_{tableAttribute.Name}_{indexName} ON {tableAttribute.Name} ({columnName})");
                    }
                    else
                    {
                        indexSql.Add($"CREATE INDEX IF NOT EXISTS IX_{tableAttribute.Name}_{indexName} ON {tableAttribute.Name} ({columnName})");
                    }
                }
            }

            using SqliteConnection connection = await GetConnectionAsync();
            SqliteTransaction transaction = connection.BeginTransaction();

            await ExecuteSqlAsync(tableBuilder.ToString(), null, transaction);

            foreach (string sql in indexSql)
            {
                await ExecuteSqlAsync(sql, null, transaction);
            }

            transaction.Commit();
        }
    }
}
