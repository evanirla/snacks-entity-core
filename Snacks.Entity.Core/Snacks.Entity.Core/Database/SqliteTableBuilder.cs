using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snacks.Entity.Core.Database
{
    /// <summary>
    /// 
    /// </summary>
    public class SqliteTableBuilder : BaseTableBuilder<SqliteService, SqliteConnection>
    {
        public SqliteTableBuilder(IDbService<SqliteConnection> dbService) : base(dbService)
        {
            
        }

        public override async Task CreateTableAsync<T>()
        {
            TableMapping table = GetTableMapping<T>();

            StringBuilder tableBuilder = new StringBuilder();

            tableBuilder.Append($"CREATE TABLE IF NOT EXISTS {table.Name} (");

            foreach (TableColumnMapping column in table.Columns)
            {
                tableBuilder.Append($"{column.Name} {column.TypeName}");

                if (column.IsKey)
                {
                    tableBuilder.Append(" PRIMARY KEY");

                    if (column.IsDatabaseGenerated &&
                        column.DatabaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                    {
                        if (column.TypeName.ToUpper().Contains("INT"))
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

            using SqliteConnection connection = await _dbService.GetConnectionAsync();

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
    }
}
