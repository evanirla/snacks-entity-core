using System;

namespace Snacks.Entity.Authorization.Attributes
{
    public class PolicyAttribute : Attribute
    {
        public string Name { get; set; }

        public PolicyAttribute(string name)
        {
            Name = name;
        }
    }
}
