﻿using Snacks.Entity.Core.Attributes;
using Snacks.Entity.Core.Entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestApplication.Models
{
    [Serializable]
    public class CustomerModel : BaseEntityModel<int>
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Unique]
        public string ExternalId { get; set; } = Guid.NewGuid().ToString();
    }
}
