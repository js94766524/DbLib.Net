using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DBLib
{
    public static class DbConnectionExtends
    {
        /// <summary>
        /// 尝试打开数据库链接
        /// </summary>
        /// <returns>True:正常 False:异常</returns>
        public static bool TryOpen(this DbConnection conn)
        {
            try
            {
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 批量执行SQL语句，若发生异常则会全部回滚
        /// </summary>
        public static void ExecuteSqlList(this DbConnection conn, List<string> sqls)
        {
            if (conn.TryOpen())
            {
                string sql = string.Empty;
                DbTransaction trans = conn.BeginTransaction();
                DbCommand cmd = conn.CreateCommand();
                cmd.Transaction = trans;
                try
                {
                    for (int i = 0; i < sqls.Count; i++)
                    {
                        sql = sqls[i];
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch (Exception e)
                {
                    trans.Rollback();
                    throw new Exception($"执行SQL语句时发生异常：SQL = {sql}", e);
                }
            }
        }
        
    }
}
