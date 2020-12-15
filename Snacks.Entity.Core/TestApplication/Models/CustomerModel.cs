﻿using Snacks.Entity.Core.Entity;
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
        public int CustomerId { get; set; }
    }
}
