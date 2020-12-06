using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Snacks.Entity.Core.Entity
{
    public class BaseEntityModel<TKey> : IEntityModel<TKey>
    {
        [Key]
        TKey Id { get; set; }

        [NotMapped]
        public string IdempotencyKey { get; set; }
    }
}
