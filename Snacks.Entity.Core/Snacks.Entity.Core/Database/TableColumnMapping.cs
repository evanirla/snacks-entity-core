using Snacks.Entity.Core.Attributes;
using Snacks.Entity.Core.Entity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Snacks.Entity.Core.Database
{
    public class TableColumnMapping
    {
        public PropertyInfo Property { get; set; }
        public PropertyInfo ToModelProperty { get; set; }
        public ColumnAttribute ColumnAttribute { get; set; }
        public DatabaseGeneratedAttribute DatabaseGeneratedAttribute { get; set; }
        public KeyAttribute KeyAttribute { get; set; }
        public ForeignKeyAttribute ForeignKeyAttribute { get; set; }
        public IndexedAttribute IndexedAttribute { get; set; }
        public RequiredAttribute RequiredAttribute { get; set; }
        public MaxLengthAttribute MaxLengthAttribute { get; set; }

        public string Name => ColumnAttribute?.Name ?? Property.Name;
        public bool IsKey => KeyAttribute != null;
        public bool IsForeignKey => ForeignKeyAttribute != null;
        public bool IsDatabaseGenerated => DatabaseGeneratedAttribute != null &&
            DatabaseGeneratedAttribute.DatabaseGeneratedOption != DatabaseGeneratedOption.None;
        public bool IsIndexed => IndexedAttribute != null;
        public bool IsRequired => RequiredAttribute != null;
        public bool IsUnique => IndexedAttribute.Unique;
        public Type ForeignType => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IEntityModel).IsAssignableFrom(x))
            .FirstOrDefault(x => x.Name == ForeignKeyAttribute?.Name);
        public TableMapping ForeignTable => TableMapping.GetMapping(ForeignType);
        public TableColumnMapping ForeignColumn => ForeignTable.Columns.FirstOrDefault(x => x.IsKey);

        public string TypeName
        {
            get
            {
                if (ColumnAttribute?.TypeName != null)
                {
                    return ColumnAttribute?.TypeName;
                }

                Type propertyType = Nullable.GetUnderlyingType(Property.PropertyType) ?? Property.PropertyType;

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

                // TODO: Should an exception be raised?
                return null;
            }
        }

        public object GetValue(object obj)
        {
            return Property.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            Property.SetValue(obj, value);
        }

        public object GetDefaultValue()
        {
            if (Property.PropertyType.IsValueType)
            {
                return Activator.CreateInstance(Property.PropertyType);
            }

            return null;
        }
    }
}
