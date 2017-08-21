using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public int Version { get; set; }

        public bool MultiPrimaryKey { get; set; }

        public TableAttribute(string Name)
        {
            this.Name = Name;
        }

       
    }
}
