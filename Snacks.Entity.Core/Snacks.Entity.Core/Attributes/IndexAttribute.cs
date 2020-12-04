using System;
using System.Collections.Generic;
using System.Text;

namespace Snacks.Entity.Core.Attributes
{
    public class IndexAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsUnique { get; set; }

        public IndexAttribute()
        {

        }

        public IndexAttribute(string name)
        {
            Name = name;
        }
    }
}
