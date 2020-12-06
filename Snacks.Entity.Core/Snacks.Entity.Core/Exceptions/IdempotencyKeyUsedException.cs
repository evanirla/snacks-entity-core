using System;
using System.Runtime.Serialization;

namespace Snacks.Entity.Core.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class IdempotencyKeyUsedException : Exception, ISerializable
    {
    }
}
