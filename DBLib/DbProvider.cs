using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DBLib
{
    /// <summary>
    /// DbProviderFactory类的装饰类，用于为DbProviderFactory增加新的方法
    /// </summary>
    public class DbProvider
    {
        public DbProvider( DbProviderFactory original )
        {
            Original = original ?? throw new ArgumentNullException("original");
        }
        
        /// <summary>
        /// 被装饰的原始的DbProviderFactory实例
        /// </summary>
        public DbProviderFactory Original { get; private set; }

        /// <summary>
        /// 获取一个数据库链接最后一次插入数据的id
        /// （如需要此功能需要重写此方法，否则默认返回-1）
        /// </summary>
        /// <param name="conn">数据库链接</param>
        /// <returns>最后一次插入数据的id</returns>
        public virtual int GetLastInsertedId( DbConnection conn )
        {
            return -1;
        }
    }
}
