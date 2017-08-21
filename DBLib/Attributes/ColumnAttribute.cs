using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DBLib.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string Name { get; }

        public DbType DataType { get; private set; }

        //public int Length { get; set; }

        public bool PrimaryKey { get; set; }

        public bool AutoIncrement { get; set; }

        //public bool Unique { get; set; }

        //public bool NotNull { get; set; }

        //public object Default { get; set; }

        //public int Version { get; set; }

        public ColumnAttribute(string name, DbType dataType)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            Name = name;
            DataType = dataType;
        }
    }

   
}
