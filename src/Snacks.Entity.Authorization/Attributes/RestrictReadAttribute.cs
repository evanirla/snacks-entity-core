using System;

namespace Snacks.Entity.Authorization.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestrictReadAttribute : RestrictAttributeBase
    {
        public RestrictReadAttribute(params string[] roles) : base(roles)
        {
            
        }
    }
}
