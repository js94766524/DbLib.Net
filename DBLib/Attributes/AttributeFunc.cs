using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DBLib.Attributes
{
    /// <summary>
    /// 应用了Attribute技术的静态方法类
    /// </summary>
    public static class AttributeFunc
    {
        #region 获取Attribute对象的方法

        /// <summary>
        /// 获取TableAttribute指示的Class或Struct中被ColumnAttribute标注的属性
        /// </summary>
        public static List<ColumnAttribute> GetColumnList( Type t )
        {
            List<ColumnAttribute> list = new List<ColumnAttribute>();
            var props = t.GetProperties();
            foreach (var p in props)
            {
                var columnAttrArr = p.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrArr.Length == 1) list.Add(columnAttrArr[0] as ColumnAttribute);
                else continue;
            }

            if (list.Count > 0)
            {
                return list;
            }
            else
            {
                throw new ArgumentException($"{t.FullName}中没有找到被指定ColumnAttribute的属性");
            }
        }

        /// <summary>
        /// 获取TableAttribute指示的Class或Struct中被ColumnAttribute标注的PropertyInfo和对应ColumnAttribute的集合
        /// </summary>
        public static Dictionary<PropertyInfo, ColumnAttribute> GetColumnDictionary( Type t )
        {
            Dictionary<PropertyInfo, ColumnAttribute> dict = new Dictionary<PropertyInfo, ColumnAttribute>();
            var props = t.GetProperties();
            foreach (var p in props)
            {
                var columnAttrArr = p.GetCustomAttributes(typeof(ColumnAttribute), true);
                if (columnAttrArr.Length == 1) dict[p] = columnAttrArr[0] as ColumnAttribute;
                else continue;
            }

            if (dict.Count > 0)
            {
                return dict;
            }
            else
            {
                throw new ArgumentException($"{t.FullName}中没有找到被指定ColumnAttribute的属性");
            }
        }

        /// <summary>
        /// 获取Class或Struct的TableAttribute
        /// </summary>
        public static TableAttribute GetTableAttribute( Type t )
        {
            var As = t.GetCustomAttributes(typeof(TableAttribute), true);

            if (As.Length == 1)
            {
                return As[0] as TableAttribute;
            }
            else throw new ArgumentException($"{t.FullName}没有被指定TableAttribute");
        }

        #endregion

        #region 通过Attribute获取SQL语句


        /// <summary>
        /// 获取Attribute标注的插入语句
        /// <para>当大批量生成时推荐使用GetInsertSQL(object, TableAttribute, Dictionary)</para>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetInsertSQL( object model )
        {
            Type type = model.GetType();
            TableAttribute tableAttr = GetTableAttribute(type);
            var dict = GetColumnDictionary(type);
            return GetInsertSQL(model, tableAttr, dict);
        }

        /// <summary>
        /// 获取Attribute标注的插入语句
        /// </summary>
        public static string GetInsertSQL( object model, TableAttribute table, Dictionary<PropertyInfo, ColumnAttribute> columnDict )
        {
            StringBuilder columns = new StringBuilder();
            StringBuilder values = new StringBuilder();
            foreach (var kv in columnDict)
            {
                PropertyInfo p = kv.Key;
                ColumnAttribute col = kv.Value;
                if (col.PrimaryKey && col.AutoIncrement)
                {
                    continue;
                }
                else
                {
                    var value = p.GetValue(model, null);
                    if (value == null) continue;
                    string valueStr = value.ToString();
                    if (col.DataType == DbType.Boolean)
                    {
                        valueStr = (bool)value ? "1" : "0";
                    }
                    else if (!double.TryParse(valueStr, out double d))
                    {
                        valueStr = $"'{valueStr}'";
                    }
                    columns.Append(col.Name).Append(",");
                    values.Append(valueStr).Append(",");
                }
            }
            columns.Remove(columns.Length - 1, 1);
            values.Remove(values.Length - 1, 1);

            return $"insert into {table.Name} ({columns.ToString()}) values ({values.ToString()});";
        }

        /// <summary>
        /// 获取Attribute标注的更新语句
        /// <para>当大批量生成时推荐使用GetUpdateSQL(object, TableAttribute, Dictionary)</para>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetUpdateSQL( object model )
        {
            Type type = model.GetType();
            TableAttribute tableAttr = GetTableAttribute(type);
            var dict = GetColumnDictionary(type);
            return GetUpdateSQL(model, tableAttr, dict);
        }

        /// <summary>
        /// 获取Attribute标注的更新语句
        /// </summary>
        /// <param name="model"></param>
        /// <param name="table"></param>
        /// <param name="columnDict"></param>
        /// <returns></returns>
        public static string GetUpdateSQL( object model, TableAttribute table, Dictionary<PropertyInfo, ColumnAttribute> columnDict )
        {
            StringBuilder keyValues = new StringBuilder();
            StringBuilder condition = new StringBuilder();
            foreach (var kv in columnDict)
            {
                PropertyInfo p = kv.Key;
                ColumnAttribute col = kv.Value;

                var value = p.GetValue(model, null);
                string valueStr;
                if (value == null)
                {
                    valueStr = "NULL";
                }
                else if (col.DataType == DbType.Boolean)
                {
                    valueStr = (bool)value ? "1" : "0";
                }
                else if (!double.TryParse(value.ToString(), out double d))
                {
                    valueStr = $"'{value.ToString()}'";
                }
                else
                {
                    valueStr = value.ToString();
                }

                string kvStr = $"{col.Name} = {valueStr}";

                if (col.PrimaryKey)
                {
                    condition.Append(" AND ").Append(kvStr);
                }
                else
                {
                    keyValues.Append(", ").Append(kvStr);
                }
            }
            if (keyValues.Length > 2) keyValues.Remove(0, 2);
            if (condition.Length > 5) condition.Remove(0, 5);

            StringBuilder sql = new StringBuilder("UPDATE ");
            sql.Append(table.Name).Append(" SET ").Append(keyValues);
            if (condition.Length != 0) sql.Append($" WHERE {condition.ToString()}");
            sql.Append(";");
            return sql.ToString();
        }

        /// <summary>
        /// 获取Attribute标注的插入语句，其中value为字段名前缀@
        /// <para>例如：“INSERT INTO TABLE1 (NAME,AGE) VALUES (@NAME,@AGE);”</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnDict">属性字段标注集合</param>
        /// <returns>SQL语句</returns>
        public static string GetInsertSQL_UseParameters( string tableName, Dictionary<PropertyInfo, ColumnAttribute> columnDict )
        {
            string sql = "INSERT INTO {0} ({1}) VALUES ({2});";
            StringBuilder cols = new StringBuilder();
            StringBuilder vals = new StringBuilder();
            foreach (var kv in columnDict)
            {
                PropertyInfo p = kv.Key;
                ColumnAttribute col = kv.Value;
                if (col.PrimaryKey && col.AutoIncrement)
                {
                    continue;
                }
                else
                {
                    cols.Append(col.Name).Append(",");
                    vals.Append("@").Append(col.Name).Append(",");
                }
            }
            if (cols.Length > 0)
            {
                cols.Remove(cols.Length - 1, 1);
                vals.Remove(vals.Length - 1, 1);
            }
            return string.Format(sql, tableName, cols.ToString(), vals.ToString());
        }

        /// <summary>
        /// 获取Attribute标注的更新语句，其中value为字段名前缀@
        /// <para>例如：“UPDATE TABLE1 SET ID = @ID, NAME = @NAME WHERE ID = @ID;”</para>
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnDict">属性字段标注集合</param>
        /// <returns>SQL语句</returns>
        public static string GetUpdateSQL_UseParameters( string tableName, Dictionary<PropertyInfo, ColumnAttribute> columnDict )
        {
            string sql = "UPDATE {0} SET {1} WHERE {2};";
            StringBuilder set = new StringBuilder();
            StringBuilder where = new StringBuilder();
            foreach (var kv in columnDict)
            {
                PropertyInfo p = kv.Key;
                ColumnAttribute col = kv.Value;
                if (col.PrimaryKey)
                {
                    where.Append(" AND ").Append(col.Name).Append(" = @").Append(col.Name);
                }
                else
                {
                    set.Append(", ").Append(col.Name).Append(" = @").Append(col.Name);
                }
            }
            if (set.Length > 0) set.Remove(0, 2);

            if (where.Length > 0) where.Remove(0, 5);

            return string.Format(sql, tableName, set.ToString(), where.ToString());
        }

        /// <summary>
        /// 获取Attribute标注的属性，转换为DbParameter数组
        /// </summary>
        /// <param name="model">数据model</param>
        /// <param name="factory">用于创建DbParameter的实现类对象</param>
        /// <param name="columnDict">属性字段标注集合</param>
        /// <returns>DbParameter数组</returns>
        public static DbParameter[] GetDbParametesr( object model, DbProviderFactory factory, Dictionary<PropertyInfo, ColumnAttribute> columnDict )
        {
            List<DbParameter> parameters = new List<DbParameter>();
            foreach (var kv in columnDict)
            {
                DbParameter t = factory.CreateParameter();
                t.ParameterName = $"@{kv.Value.Name}";
                t.DbType = kv.Value.DataType;
                t.Value = kv.Key.GetValue(model, null);
                parameters.Add(t);
            }
            return parameters.ToArray();
        }

        #endregion

        #region 通过Attribute进行数据映射

        /// <summary>
        /// 通过Attribute来映射数据库表到Model列表
        /// </summary>
        public static List<T> TransToModelList<T>( DataTable table )
        {
            Type type = typeof(T);
            Dictionary<PropertyInfo, ColumnAttribute> dict = GetColumnDictionary(type);
            return TransToModelList<T>(table, dict);
        }

        /// <summary>
        /// 通过Attribute来映射数据库表到Model列表
        /// </summary>
        public static List<T> TransToModelList<T>( DataTable table, Dictionary<PropertyInfo, ColumnAttribute> dict )
        {
            List<T> list = new List<T>();
            if (table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    T obj = TransToModel<T>(row, dict);
                    list.Add(obj);
                }
            }
            return list;
        }

        /// <summary>
        /// 通过Attribute来映射数据库表的一行数据到Model实例。
        /// <para>推荐使用GetModelList(DataTable)方法或者GetModel(DataRow,Dictionary)代替。</para>
        /// </summary>
        public static T TransToModel<T>( DataRow row )
        {
            Type type = typeof(T);
            Dictionary<PropertyInfo, ColumnAttribute> dict = GetColumnDictionary(type);
            return TransToModel<T>(row, dict);
        }

        /// <summary>
        /// 通过Attribute来映射数据库表的一行数据到Model实例。
        /// </summary>
        public static T TransToModel<T>( DataRow row, Dictionary<PropertyInfo, ColumnAttribute> dict )
        {
            Type type = typeof(T);
            T obj = (T)type.Assembly.CreateInstance(type.FullName);

            foreach (var keyValue in dict)
            {
                var v = Convert.ChangeType(row[keyValue.Value.Name], keyValue.Key.PropertyType);
                keyValue.Key.SetValue(obj, v, null);
            }

            return obj;
        }

        /// <summary>
        /// 通过Attribute将model对象转换为Json字符串
        /// </summary>
        /// <param name="model">数据模型对象</param>
        /// <returns>Json字符串</returns>
        public static string TransToJsonString( object model )
        {
            Type type = model.GetType();
            var dict = GetColumnDictionary(type);
            return TransToJsonString(model, dict);
        }

        /// <summary>
        /// 通过Attribute将model对象转换为Json字符串
        /// </summary>
        public static string TransToJsonString( object model, Dictionary<PropertyInfo, ColumnAttribute> dict )
        {
            StringBuilder json = new StringBuilder("{");

            foreach(var kv in dict)
            {
                PropertyInfo p = kv.Key;
                ColumnAttribute col = kv.Value;
                json.Append($"\"{col.Name}\"=");
                var value = p.GetValue(model, null);

                if (value == null)
                {
                    json.Append("null,");
                    continue;
                }
                else if(value is string)
                {
                    json.Append($"\"{value}\",");
                }
                else if (double.TryParse(value.ToString(), out double d))
                {
                    json.Append(value.ToString()).Append(",");
                }
                else
                {
                    json.Append($"\"{value}\",");
                }
            }
            if (json.Length > 1) json.Remove(json.Length - 1, 1);
            json.Append("}");
            return json.ToString();
        }

        #endregion
    }
}
