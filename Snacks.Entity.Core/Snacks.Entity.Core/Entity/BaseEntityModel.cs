using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Snacks.Entity.Core.Entity
{
    [Serializable]
    public class BaseEntityModel<TKey> : IEntityModel<TKey>
    {
        PropertyInfo KeyProperty => GetType().GetProperties()
            .FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));

        [NotMapped]
        public string IdempotencyKey { get; set; }

        [NotMapped]
        public TKey Key
        {
            get
            {
                return (TKey)KeyProperty.GetValue(this);
            }
            set
            {
                KeyProperty.SetValue(this, value);
            }
        }

        object IEntityModel.Key
        {
            get => Key;
            set => Key = (TKey)value;
        }

        public string TableName => GetType().GetCustomAttribute<TableAttribute>()?.Name ?? GetType().Name;
    }
}
