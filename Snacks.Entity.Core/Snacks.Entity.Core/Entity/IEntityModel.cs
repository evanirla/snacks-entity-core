using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Snacks.Entity.Core.Entity
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The property type of the primary key.</typeparam>
    public interface IEntityModel<TKey>
    {
        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        string IdempotencyKey { get; set; }

        void SetKey(TKey key);
    }
}
