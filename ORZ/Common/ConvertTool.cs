using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORZ
{
    public static class ConvertTool
    {
        /// <summary>
        /// 数据类型转换
        /// </summary>
        public static object ChangeType( object value, Type type )
        {
            //如果是Nullable类型，获取Nullable<>的内部类型
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];

            //如果是枚举类型
            if (typeof(Enum).IsAssignableFrom(type))
            {
                return Enum.Parse(type, value.ToString(), true);
            }
            else return Convert.ChangeType(value, type);
        }
    }
}
