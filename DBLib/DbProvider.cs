using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DBLib
{
    public abstract class DbProvider
    {
        public DbProvider( DbProviderFactory original )
        {
            Original = original ?? throw new ArgumentNullException("original");
        }

        public DbProviderFactory Original { get; private set; }

        /// <summary>
        /// 获取一个数据库链接最后一次插入数据的id
        /// </summary>
        /// <param name="conn">数据库链接</param>
        /// <returns>最后一次插入数据的id</returns>
        public abstract long GetLastInsertedId( DbConnection conn );
    }
}
