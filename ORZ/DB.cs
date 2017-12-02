using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace ORZ
{
    public class DB
    {
        #region 静态属性和方法

        /// <summary>
        /// 可操作的数据库集合
        /// </summary>
        public static Dictionary<string, DB> Databases = new Dictionary<string, DB>();

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="Provider">数据库类型对应的Provider</param>
        /// <param name="ConnectionString">数据库链接字符串</param>
        /// <param name="DBName">数据库名称，和Model中<see cref="TableAttribute.DBName"/>对应。默认为default</param>
        /// <returns><see cref="DB"/>对象</returns>
        public static DB Open( DbProviderFactory Provider, string ConnectionString, string DBName = "default" )
        {
            DB db = null;
            if (Databases.ContainsKey(DBName)) db = Databases[DBName];
            else Databases[DBName] = db = new DB();

            if (db.Provider == null && Provider == null)
                throw new ArgumentNullException("Provider");
            db.Provider = Provider;

            if (string.IsNullOrWhiteSpace(db.ConnectionString) && string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException("Connection String");

            using (var conn = Provider.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                db.ConnectionString = ConnectionString;
            }
            return db;
        }

        /// <summary>
        /// 打开数据库
        /// </summary>
        /// <param name="ProviderName">数据库类型对应的ProviderName</param>
        /// <param name="ConnectionString">数据库链接字符串</param>
        /// <param name="DBName">数据库名称，和Model中<see cref="TableAttribute.DBName"/>对应。默认为default</param>
        /// <returns><see cref="DB"/>对象</returns>
        public static DB Open( string ProviderName, string ConnectionString, string DBName = "default" )
        {
            var factory = DbProviderFactories.GetFactory(ProviderName);
            return Open(factory, ConnectionString, DBName);
        }

        /// <summary>
        /// 列举当前数据库连接信息
        /// </summary>
        /// <returns>文字信息（多行）</returns>
        public static string ListDatabases()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("总计连接数据库").Append(Databases.Count).AppendLine("个").AppendLine("----------");
            if (Databases.Count > 0)
            {
                foreach (var kv in Databases)
                {
                    DB db = kv.Value;
                    sb
                        .Append("DBName:").Append(kv.Key)
                        .Append("  Provider:").Append(db.Provider.GetType().FullName)
                        .Append("  ConnectionString:").AppendLine(db.ConnectionString);
                }
                sb.AppendLine("----------");
            }
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        private string ConnectionString { get; set; }

        /// <summary>
        /// 数据库实现的DbProviderFactory
        /// </summary>
        public DbProviderFactory Provider { get; private set; }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        public DbConnection CreateConn()
        {
            var conn = Provider.CreateConnection();
            conn.ConnectionString = ConnectionString;
            return conn;
        }

        #region 基本数据库操作

        /// <summary>
        /// 执行非查询类sql语句，不关闭DbConnection
        /// </summary>
        public int ExecuteNonQuery( string sql, DbConnection conn, params DbParameter[] parameters )
        {
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            Debug.WriteLine(sql);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 执行非查询类sql语句，完毕后立即释放使用的DbConnection
        /// </summary>
        public int ExecuteNonQuery( string sql, params DbParameter[] parameters )
        {
            using (var conn = CreateConn())
            {
                return ExecuteNonQuery(sql, CreateConn(), parameters);
            }
        }

        /// <summary>
        /// 执行查询类sql语句，返回DataSet，不关闭DbConnection
        /// </summary>
        public DataSet ExecuteDataSet( string sql, DbConnection conn, params DbParameter[] parameters )
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            DataSet set = new DataSet();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            DbDataAdapter adapter = Provider.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(set);
            Debug.WriteLine(sql);
            return set;
        }

        /// <summary>
        /// 执行查询类sql语句，返回DataSet，完毕后立即释放使用的DbConnection
        /// </summary>
        public DataSet ExecuteDataSet( string sql, params DbParameter[] parameters )
        {
            using (var conn = CreateConn())
            {
                return ExecuteDataSet(sql, conn, parameters);
            }
        }

        /// <summary>
        /// 执行查询类sql语句，返回DataReader，不关闭DbConnection
        /// </summary>
        public IDataReader ExecuteDataReader( string sql, DbConnection conn, CommandBehavior behavior, params DbParameter[] parameters )
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            Debug.WriteLine(sql);
            return cmd.ExecuteReader(behavior);
        }

        /// <summary>
        /// 执行查询类sql语句，返回DataReader，完毕后立即释放使用的DbConnection
        /// </summary>
        public IDataReader ExecuteDataReader( string sql, params DbParameter[] parameters )
        {
            using (var conn = CreateConn())
            {
                return ExecuteDataReader(sql, conn, CommandBehavior.Default, parameters);
            }
        }

        /// <summary>
        /// 执行scalar查询，不关闭DbConnection
        /// </summary>
        public object ExecuteScalar( string sql, DbConnection conn, params DbParameter[] parameters )
        {
            if (conn.State != ConnectionState.Open) conn.Open();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            Debug.WriteLine(sql);
            return cmd.ExecuteScalar();
        }

        /// <summary>
        /// 执行scalar查询，完毕后立即释放使用的DbConnection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object ExecuteScalar( string sql, params DbParameter[] parameters )
        {
            using (var conn = CreateConn())
            {
                return ExecuteScalar(sql, conn, parameters);
            }
        }

        #endregion
    }
}
