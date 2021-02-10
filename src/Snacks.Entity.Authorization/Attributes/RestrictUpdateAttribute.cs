using System;

namespace Snacks.Entity.Authorization.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestrictUpdateAttribute : RestrictAttributeBase
    {
        public RestrictUpdateAttribute(params string[] roles) : base(roles)
        {
            
        }
    }
}
