using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORZ
{
    /// <summary>
    /// 字段信息
    /// </summary>
    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsAutoIncreament { get; set; }
        public object DefaultValue { get; set; }
        public object Value { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }
}
