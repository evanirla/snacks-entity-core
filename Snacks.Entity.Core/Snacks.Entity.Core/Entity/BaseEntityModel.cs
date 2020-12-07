using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Snacks.Entity.Core.Entity
{
    public class BaseEntityModel<TKey> : IEntityModel<TKey>
    {
        [NotMapped]
        public string IdempotencyKey { get; set; }

        public void SetKey(TKey key)
        {
            PropertyInfo keyProperty = GetType().GetProperties()
                .FirstOrDefault(x => x.IsDefined(typeof(KeyAttribute)));

            keyProperty.SetValue(this, key);
        }
    }
}
