using DBLib.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBLib
{
    public class DbMaster
    {
        private DbConnectionStringBuilder connParams;
        public DbProvider Provider { get; private set; }

        /// <summary>
        /// 构造
        /// </summary>
        public DbMaster( DbProvider provider )
        {
            Provider = provider ?? throw new ArgumentNullException("provider");
            connParams = Provider.Original.CreateConnectionStringBuilder();
        }

        /// <summary>
        /// 数据库链接参数
        /// </summary>
        public DbConnectionStringBuilder ConnParams { get { return connParams; } }

        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        public string ConnString { get { return ConnParams.ToString(); } }


        #region 基本增删改查方法

        /// <summary>
        /// 获取一个已经打开的数据库连接
        /// </summary>
        public DbConnection GetOpenedConn()
        {
            var conn = Provider.Original.CreateConnection();
            if (conn.TryOpen()) return conn;
            else throw new Exception("无法打开数据库。");
        }

        /// <summary>
        /// 创建数据库参数
        /// </summary>
        /// <returns>数据库参数对象</returns>
        public DbParameter CreateParameter()
        {
            return Provider.Original.CreateParameter();
        }

        /// <summary>
        /// 执行非查询类sql语句
        /// </summary>
        public int ExecuteNonQuery( string sql, params DbParameter[] parameters )
        {
            var conn = GetOpenedConn();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            int i = cmd.ExecuteNonQuery();
            conn.Close();
            return i;
        }

        /// <summary>
        /// 批量执行带参数的查询类sql语句
        /// </summary>
        /// <param name="dict">key:sql语句 value:DbParameter数组</param>
        public void ExecuteNonQuerys( Dictionary<string, DbParameter[]> dict )
        {
            var conn = GetOpenedConn();
            var trans = conn.BeginTransaction();
            DbCommand cmd = conn.CreateCommand();
            try
            {
                foreach (var kv in dict)
                {
                    cmd.CommandText = kv.Key;
                    cmd.Parameters.Clear();
                    if (kv.Value != null) cmd.Parameters.AddRange(kv.Value);
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch (Exception e)
            {
                trans.Rollback();
                throw e;
            }
            finally
            {
                conn?.Close();
            }

        }

        /// <summary>
        /// 执行查询类sql语句，返回DataSet
        /// </summary>
        public DataSet ExecuteDataSet( string sql )
        {
            DataSet set = new DataSet();
            var conn = GetOpenedConn();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            DbDataAdapter adapter = Provider.Original.CreateDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(set);
            conn.Close();
            return set;
        }

        /// <summary>
        /// 执行查询类sql语句，返回DataReader
        /// </summary>
        public IDataReader ExecuteDataReader( string sql )
        {
            var conn = GetOpenedConn();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// 执行scalar查询
        /// </summary>
        public object ExecuteScalar( string sql )
        {
            var conn = GetOpenedConn();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var obj = cmd.ExecuteScalar();
            conn.Close();
            return obj;
        }

        /// <summary>
        /// 执行insert语句，并返回插入数据的id
        /// </summary>
        /// <param name="sql">insert sql</param>
        /// <param name="parameters">参数</param>
        /// <returns>插入数据的id</returns>
        public long ExecuteInsert( string sql, params DbParameter[] parameters )
        {
            var conn = GetOpenedConn();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            int i = cmd.ExecuteNonQuery();
            long id = i > 0 ? Provider.GetLastInsertedId(conn) : -1;
            conn.Close();
            return id;
        }
        #endregion

        #region 使用Attribute技术反射的方法

        /// <summary>
        /// 批量插入数据（速度快，费内存）
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <param name="modelList">model集合</param>
        public void InsertModelList<D>( IEnumerable<D> modelList )
        {
            if (modelList.Count() == 0) return;

            Type type = modelList.First().GetType();
            TableAttribute tableAttr = AttributeFunc.GetTableAttribute(type);
            Dictionary<PropertyInfo, ColumnAttribute> dict = AttributeFunc.GetColumnDictionary(type);

            List<string> sqlList = new List<string>();
            foreach (object item in modelList)
            {
                sqlList.Add(AttributeFunc.GetInsertSQL(item, tableAttr, dict));
            }

            var conn = GetOpenedConn();
            conn.ExecuteSqlList(sqlList);
            conn.Close();
        }

        /// <summary>
        /// 批量插入数据（省内存，速度慢）
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <typeparam name="P">DbParameter实现类</typeparam>
        /// <param name="modelList">model集合</param>
        public void InsertModelList_UseParameter<D>( IEnumerable<D> modelList )
        {
            Type type = modelList.First().GetType();
            TableAttribute tableAttr = AttributeFunc.GetTableAttribute(type);
            Dictionary<PropertyInfo, ColumnAttribute> dict = AttributeFunc.GetColumnDictionary(type);
            string sql = AttributeFunc.GetInsertSQL_UseParameters(tableAttr.Name, dict);
            LargeAmoutInsertOrUpdate<D>(sql, modelList, dict);
        }

        /// <summary>
        /// 批量更新数据 （速度快，费内存）
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <param name="modelList">model集合</param>
        public void UpdateModelList<D>( IEnumerable<D> modelList )
        {
            if (modelList.Count() == 0) return;

            Type type = typeof(D);
            TableAttribute tableAttr = AttributeFunc.GetTableAttribute(type);
            Dictionary<PropertyInfo, ColumnAttribute> dict = AttributeFunc.GetColumnDictionary(type);

            List<string> sqlList = new List<string>();
            foreach (var item in modelList)
            {
                sqlList.Add(AttributeFunc.GetUpdateSQL(item, tableAttr, dict));
            }

            var conn = GetOpenedConn();
            conn.ExecuteSqlList(sqlList);
            conn.Close();
        }

        /// <summary>
        /// 批量插入数据（省内存，速度慢）
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <typeparam name="P">DbParameter实现类</typeparam>
        /// <param name="modelList">model集合</param>
        public void UpdateModelList_UseParameter<D>( IEnumerable<D> modelList )
        {
            Type type = modelList.First().GetType();
            TableAttribute tableAttr = AttributeFunc.GetTableAttribute(type);
            Dictionary<PropertyInfo, ColumnAttribute> dict = AttributeFunc.GetColumnDictionary(type);
            string sql = AttributeFunc.GetUpdateSQL_UseParameters(tableAttr.Name, dict);
            LargeAmoutInsertOrUpdate<D>(sql, modelList, dict);
        }

        /// <summary>
        /// 大批量重复执行插入或更新
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <typeparam name="P">DbParameter实现类</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="modelList">model集合</param>
        /// <param name="dict">属性字段标注集合</param>
        public void LargeAmoutInsertOrUpdate<D>( string sql, IEnumerable<D> modelList, Dictionary<PropertyInfo, ColumnAttribute> dict )
        {
            if (modelList.Count() == 0) return;

            var conn = GetOpenedConn();
            var trans = conn.BeginTransaction();
            var cmd = conn.CreateCommand();
            cmd.Transaction = trans;
            cmd.CommandText = sql;

            try
            {
                foreach (var item in modelList)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(AttributeFunc.GetDbParametesr(item, Provider.Original, dict));
                    cmd.ExecuteNonQuery();
                }

                trans.Commit();
            }
            catch
            {
                trans?.Rollback();
                throw;
            }
            finally
            {
                conn?.Close();
            }
        }

        /// <summary>
        /// 查询数据模型列表
        /// </summary>
        /// <typeparam name="D">需要标注TableAttribute且拥有标注ColumnAttribute的属性</typeparam>
        /// <param name="condition">条件语句，例如："where AGE > 20"</param>
        /// <returns>model集合</returns>
        public List<D> QueryModelList<D>( string condition )
        {
            Type type = typeof(D);
            TableAttribute tableAttr = AttributeFunc.GetTableAttribute(type);
            Dictionary<PropertyInfo, ColumnAttribute> dict = AttributeFunc.GetColumnDictionary(type);

            StringBuilder sql = new StringBuilder("SELECT ");
            foreach (var kv in dict)
            {
                sql.Append(kv.Value.Name).Append(",");
            }
            sql.Remove(sql.Length - 1, 1);
            sql.Append(" FROM ").Append(tableAttr.Name);

            if (!string.IsNullOrWhiteSpace(condition)) sql.Append($" {condition}");

            DataSet set = ExecuteDataSet(sql.ToString());

            if (set.Tables.Count != 0)
            {
                return AttributeFunc.TransToModelList<D>(set.Tables[0], dict);
            }
            return null;
        }

        #endregion

    }
}
