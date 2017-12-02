using System;

namespace ORZ
{
    /// <summary>
    /// 数据库中表映射的模型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// 数据表名称
        /// </summary>
        public string TableName { get; }
        /// <summary>
        /// 调用<see cref="DB.Open(System.Data.Common.DbProviderFactory, string, string)"/>时设置的数据库名称，默认为default
        /// </summary>
        public string DBName { get; }
        
        public TableAttribute( string TableName )
        {
            this.TableName = TableName;
            this.DBName = "default";
        }

        public TableAttribute( string DBName, string TableName )
        {
            this.DBName = DBName;
            this.TableName = TableName;
        }
    }
}
