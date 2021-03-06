﻿using System;
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

        public bool PrimaryKey { get; set; }

        public bool AutoIncrement { get; set; }

        public ColumnAttribute(string name, DbType dataType)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("name");
            Name = name;
            DataType = dataType;
        }
    }

   
}
