using System;

namespace Snacks.Entity.Authorization.Attributes
{
    public abstract class RestrictAttributeBase : Attribute
    {
        public string[] Roles { get; set; }

        public RestrictAttributeBase(params string[] roles)
        {
            Roles = roles;
        }
    }
}
