using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORZ
{
    public static class ModelFactory
    {
        /// <summary>
        /// 将DataRow转换为泛型对象
        /// <para>泛型类型必须拥有无参构造器</para>
        /// </summary>
        public static T ToModel<T>( this DataRow row, Dictionary<string, ColumnInfo> cInfos ) where T : new()
        {
            T model = new T();
            var cols = row.Table.Columns;
            foreach (DataColumn col in cols)
            {
                if (cInfos.ContainsKey(col.ColumnName))
                {
                    object value = row[col.ColumnName];
                    if (!Convert.IsDBNull(value))
                    {
                        ColumnInfo i = cInfos[col.ColumnName];
                        var v = ConvertTool.ChangeType(value, i.PropertyInfo.PropertyType);
                        i.PropertyInfo.SetValue(model, v);
                    }
                }
            }
            return model;
        }

        /// <summary>
        /// 将DataRow转换为泛型对象
        /// <para>泛型类型必须拥有无参构造器</para>
        /// </summary>
        public static T ToModel<T>( this DataRow row ) where T : new()
        {
            var cInfos = DBModel.GetColumnDict(typeof(T));
            return row.ToModel<T>(cInfos);
        }

        /// <summary>
        /// 将DataTable转换为泛型对象列表
        /// <para>泛型类型必须拥有无参构造器</para>
        /// </summary>
        public static List<T> ToModelList<T>( this DataTable table ) where T : new()
        {
            List<T> list = new List<T>();
            var cInfos = DBModel.GetColumnDict(typeof(T));
            foreach (DataRow row in table.Rows)
            {
                list.Add(row.ToModel<T>(cInfos));
            }
            return list;
        }
    }
}
