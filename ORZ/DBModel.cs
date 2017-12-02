using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace ORZ
{
    /// <summary>
    /// 模型对象的扩展方法类
    /// </summary>
    public static class DBModel
    {
        #region 基本方法，获取表信息和列信息

        /// <summary>
        /// 获取Attribute标注的表信息，若未进行标注则用类名作为表名
        /// </summary>
        internal static TableInfo GetTableInfo( Type type )
        {
            TableInfo info = new TableInfo();

            var tableAttrs = type.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttrs.Length == 1)
            {
                TableAttribute ta = tableAttrs[0] as TableAttribute;
                info.DBName = ta.DBName;
                if (ta.TableName != null)
                {
                    info.TableName = ta.TableName;
                }
                else info.TableName = type.Name;
            }
            else
            {
                info.TableName = type.Name;
                info.DBName = "default";
            }
            if (!DB.Databases.ContainsKey(info.DBName)) throw new ArgumentException("未找到名为" + info.DBName + "的数据库，请修改" + type.FullName + "的TableAttribute.DBName属性");
            return info;
        }

        /// <summary>
        /// 获取Attribute标注的表信息，若未进行标注则仅用类名作为表名
        /// </summary>
        internal static TableInfo GetTableInfo( this object model )
        {
            Type type = model.GetType();
            return GetTableInfo(type);
        }

        /// <summary>
        /// 获取字段信息，若未进行标注则使用属性名称作为字段名称
        /// <para>无法获取字段值</para>
        /// </summary>
        internal static Dictionary<string, ColumnInfo> GetColumnDict( Type type )
        {

            var props = type.GetProperties();
            Dictionary<string, ColumnInfo> columnDict = new Dictionary<string, ColumnInfo>(props.Length, StringComparer.OrdinalIgnoreCase);
            foreach (var p in props)
            {
                ColumnInfo info = new ColumnInfo();
                info.PropertyInfo = p;
                var columnAttrArr = p.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrArr.Length == 1)
                {
                    ColumnAttribute ca = columnAttrArr[0] as ColumnAttribute;
                    if (ca.Ignore) continue;
                    if (ca.Name != null) info.ColumnName = ca.Name;
                    else info.ColumnName = p.Name;
                    info.DefaultValue = ca.DefaultValue;
                    info.IsAutoIncreament = ca.AutoIncrement;
                    info.IsPrimaryKey = ca.PrimaryKey;
                }
                else
                {
                    info.ColumnName = p.Name;
                }
                columnDict[info.ColumnName] = info;
            }
            return columnDict;
        }

        /// <summary>
        /// 获取Attribute标注的字段信息，若未进行标注则使用属性名称作为字段名称
        /// <para>同时获取model中的字段值</para>
        /// </summary>
        internal static List<ColumnInfo> GetColumnInfos( this object model )
        {
            List<ColumnInfo> columnList = new List<ColumnInfo>();
            Type type = model.GetType();

            var props = type.GetProperties();
            foreach (var p in props)
            {
                ColumnInfo info = new ColumnInfo();
                info.PropertyInfo = p;
                var columnAttrArr = p.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrArr.Length == 1)
                {
                    ColumnAttribute ca = columnAttrArr[0] as ColumnAttribute;
                    if (ca.Ignore) continue;
                    if (ca.Name != null) info.ColumnName = ca.Name;
                    else info.ColumnName = p.Name;
                    info.DefaultValue = ca.DefaultValue;
                    info.IsAutoIncreament = ca.AutoIncrement;
                    info.IsPrimaryKey = ca.PrimaryKey;
                    info.Value = p.GetValue(model);
                }
                else
                {
                    info.ColumnName = p.Name;
                    info.Value = p.GetValue(model);
                }
                columnList.Add(info);
            }

            return columnList;
        }

        #endregion

        #region 生成增删改SQL语句的方法

        /// <summary>
        /// 生成插入SQL
        /// </summary>
        internal static string GetInsertSql( this object model, DB db, out DbParameter[] parameters, TableInfo tInfo = null )
        {
            if (tInfo == null) tInfo = model.GetTableInfo();
            List<ColumnInfo> cInfos = model.GetColumnInfos();
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();
            List<DbParameter> paramList = new List<DbParameter>();
            foreach (var i in cInfos)
            {
                if (i.IsPrimaryKey && i.IsAutoIncreament) continue;
                columns.Append(",").Append(i.ColumnName);
                values.Append(",@").Append(i.ColumnName);
                var p = db.Provider.CreateParameter();
                p.ParameterName = "@" + i.ColumnName;
                p.Value = i.Value;
                paramList.Add(p);
            }
            if (columns.Length > 0) columns.Remove(0, 1);
            if (values.Length > 0) values.Remove(0, 1);
            parameters = paramList.ToArray();
            return $"INSERT INTO {tInfo.TableName} ({columns.ToString()}) VALUES ({values.ToString()});";
        }

        /// <summary>
        /// 生成更新SQL
        /// </summary>
        internal static string GetUpdateSql( this object model, DB db, out DbParameter[] parameters, TableInfo tInfo = null )
        {
            if (tInfo == null) tInfo = model.GetTableInfo();
            List<ColumnInfo> cInfos = model.GetColumnInfos();
            StringBuilder keyValues = new StringBuilder();
            StringBuilder condition = new StringBuilder();
            List<DbParameter> paramList = new List<DbParameter>();
            foreach (var i in cInfos)
            {
                if (i.IsPrimaryKey)
                {
                    condition.Append(" AND ").Append(i.ColumnName).Append("=@").Append(i.ColumnName);
                }
                else
                {
                    keyValues.Append(",").Append(i.ColumnName).Append("=@").Append(i.ColumnName);
                }
                var p = db.Provider.CreateParameter();
                p.ParameterName = "@" + i.ColumnName;
                p.Value = i.Value;
                paramList.Add(p);
            }
            if (condition.Length > 0) condition.Remove(0, 5);
            else throw new ArgumentException("生成SQL语句时发生异常：在" + tInfo.TableName + "表中找不到主键。");
            if (keyValues.Length > 0) keyValues.Remove(0, 1);
            parameters = paramList.ToArray();
            return $"UPDATE {tInfo.TableName} SET {keyValues.ToString()} WHERE {condition.ToString()};";
        }

        /// <summary>
        /// 生成删除SQL
        /// </summary>
        internal static string GetDeleteSql( this object model, DB db, out DbParameter[] parameters, TableInfo tInfo = null )
        {
            if (tInfo == null) tInfo = model.GetTableInfo();
            List<ColumnInfo> cInfos = model.GetColumnInfos();
            StringBuilder condition = new StringBuilder();
            StringBuilder keyValue = new StringBuilder();
            List<DbParameter> paramList = new List<DbParameter>();
            foreach (var i in cInfos)
            {
                if (i.IsPrimaryKey)
                {
                    condition.Append(" AND ").Append(i.ColumnName).Append("=@").Append(i.ColumnName);
                }
                else
                {
                    keyValue.Append(" AND ").Append(i.ColumnName).Append("=@").Append(i.ColumnName);
                }
                var p = db.Provider.CreateParameter();
                p.ParameterName = "@" + i.ColumnName;
                p.Value = i.Value;
                paramList.Add(p);
            }

            string conditionStr;
            if (condition.Length > 0) //使用主键作为删除依据
            {
                condition.Remove(0, 5);
                conditionStr = condition.ToString();
            }
            else if (keyValue.Length > 0) //当找不到主键时，使用全体字段作为删除依据
            {
                keyValue.Remove(0, 5);
                conditionStr = keyValue.ToString();
            }
            else throw new ArgumentException("生成SQL语句时发生异常：在" + tInfo.TableName + "表中找不到主键。");

            parameters = paramList.ToArray();
            return $"DELETE FROM {tInfo.TableName} WHERE {conditionStr};";
        }

        #endregion

        #region 对包外公开的增删改方法

        /// <summary>
        /// 将该Model数据插入数据库
        /// </summary>
        /// <param name="model">数据库表映射模型</param>
        public static bool Insert( this object model )
        {
            TableInfo tInfo = model.GetTableInfo();
            DB db = DB.Databases[tInfo.DBName];
            try
            {
                int i = db.ExecuteNonQuery(model.GetInsertSql(db, out DbParameter[] parameters, tInfo), parameters);
                return i == 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 更新数据库中该条数据的值
        /// </summary>
        /// <param name="model">数据库表映射模型，必须标注有主键字段</param>
        public static bool Update( this object model )
        {
            TableInfo tInfo = model.GetTableInfo();
            DB db = DB.Databases[tInfo.DBName];
            try
            {
                int i = db.ExecuteNonQuery(model.GetUpdateSql(db, out DbParameter[] parameters, tInfo), parameters);
                return i == 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// 将该Model从数据库中删除
        /// </summary>
        /// <param name="model">数据库表映射模型</param>
        public static bool Delete( this object model )
        {
            TableInfo tInfo = model.GetTableInfo();
            DB db = DB.Databases[tInfo.DBName];
            try
            {
                int i = db.ExecuteNonQuery(model.GetDeleteSql(db, out DbParameter[] parameters), parameters);
                return i == 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return false;
            }
        }

        #endregion
    }
}
