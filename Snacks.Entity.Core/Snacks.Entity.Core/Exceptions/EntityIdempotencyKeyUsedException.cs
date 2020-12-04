using System;
using System.Runtime.Serialization;

namespace Snacks.Entity.Core.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    class EntityIdempotencyKeyUsedException : Exception, ISerializable
    {
    }
}
