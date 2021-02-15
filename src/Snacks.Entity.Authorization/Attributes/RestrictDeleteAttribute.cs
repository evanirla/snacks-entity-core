using System;

namespace Snacks.Entity.Authorization.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RestrictDeleteAttribute : RestrictAttributeBase
    {
        public RestrictDeleteAttribute(params string[] roles) : base(roles)
        {
            
        }
    }
}
