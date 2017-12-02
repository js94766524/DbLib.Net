using System;

namespace ORZ
{
    /// <summary>
    /// 数据表中的字段映射的属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 指示是否为主键
        /// </summary>
        public bool PrimaryKey { get; set; }
        /// <summary>
        /// 指示是否为自增字段
        /// </summary>
        public bool AutoIncrement { get; set; }
        /// <summary>
        /// 字段默认值，默认为null
        /// </summary>
        public object DefaultValue { get; set; }
        /// <summary>
        /// 指示生成SQL语句时是否忽略该属性
        /// </summary>
        public bool Ignore { get; set; }

        public ColumnAttribute() { }
        public ColumnAttribute( string Name ) { this.Name = Name; }
    }

    /// <summary>
    /// 用于标识在生成SQL语句时需要忽略的字段
    /// </summary>
    public class IgnoreColumnAttribute : ColumnAttribute
    {
        public IgnoreColumnAttribute()
        {
            Ignore = true;
        }
    }
}
