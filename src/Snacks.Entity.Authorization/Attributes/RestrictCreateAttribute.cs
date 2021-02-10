using System;

namespace Snacks.Entity.Authorization.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestrictCreateAttribute : RestrictAttributeBase
    {
        public RestrictCreateAttribute(params string[] roles) : base(roles)
        {
            
        }
    }
}
