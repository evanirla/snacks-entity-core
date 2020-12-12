using Snacks.Entity.Core.Attributes;
using Snacks.Entity.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Snacks.Entity.Core.Database
{
    public class TableMapping
    {
        public Type ModelType { get; set; }
        public TableAttribute TableAttribute { get; set; }
        public List<TableColumnMapping> Columns { get; set; }

        public string Name => TableAttribute?.Name ?? ModelType.Name;
        public string Schema => TableAttribute?.Schema;
        public TableColumnMapping KeyColumn => Columns.FirstOrDefault(x => x.IsKey);
        public IEnumerable<IGrouping<string, TableColumnMapping>> Indexes => Columns
            .Where(x => x.IsIndexed)
            .GroupBy(x => x.IndexedAttribute.Name ?? $"IX_{Name}_{x.Name}");
        public IEnumerable<TableColumnMapping> ForeignKeyColumns => Columns
            .Where(x => x.IsForeignKey);

        public static TableMapping GetMapping<T>()
        {
            return GetMapping(typeof(T));
        }

        public static TableMapping GetMapping(Type type)
        {
            return new TableMapping
            {
                ModelType = type,
                TableAttribute = type.GetCustomAttribute<TableAttribute>(),
                Columns = type.GetProperties()
                    .Where(x => !x.IsDefined(typeof(NotMappedAttribute)))
                    .Where(x => !x.PropertyType.IsGenericType)
                    .Where(x => !typeof(IEntityModel).IsAssignableFrom(x.PropertyType))
                    .Select(x => new TableColumnMapping
                    {
                        ColumnAttribute = x.GetCustomAttribute<ColumnAttribute>(),
                        DatabaseGeneratedAttribute = x.GetCustomAttribute<DatabaseGeneratedAttribute>(),
                        IndexedAttribute =
                            x.GetCustomAttribute<UniqueAttribute>() ?? x.GetCustomAttribute<IndexedAttribute>(),
                        KeyAttribute = x.GetCustomAttribute<KeyAttribute>(),
                        ForeignKeyAttribute = x.GetCustomAttribute<ForeignKeyAttribute>(),
                        MaxLengthAttribute = x.GetCustomAttribute<MaxLengthAttribute>(),
                        Property = x,
                        RequiredAttribute = x.GetCustomAttribute<RequiredAttribute>()
                    }).ToList()
            };
        }
    }
}
