using System.Runtime.Serialization;

namespace Snacks.Entity.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The property type of the primary key.</typeparam>
    public interface IEntityModel<TKey> : ISerializable
    {
        /// <summary>
        /// 
        /// </summary>
        string IdempotencyKey { get; set; }
    }
}
