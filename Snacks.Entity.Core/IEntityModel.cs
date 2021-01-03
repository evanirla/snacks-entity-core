using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Snacks.Entity.Core
{
    public interface IEntityModel
    {
        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        string IdempotencyKey { get; set; }

        [NotMapped]
        object Key { get; set; }

        string TableName { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The property type of the primary key.</typeparam>
    public interface IEntityModel<TKey> : IEntityModel
    {
        [NotMapped]
        new TKey Key { get; set; }
    }
}
