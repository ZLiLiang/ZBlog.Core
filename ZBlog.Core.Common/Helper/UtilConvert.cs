using Newtonsoft.Json;

namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// 转换工具
    /// </summary>
    public static class UtilConvert
    {
        /// <summary>
        /// 转换为整型
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out int reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 转换为整型，失败则返回错误值
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <param name="errorValue">错误值</param>
        /// <returns></returns>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out int reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 转换为长整型
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static long ObjToLong(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out long reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 转换为金额（double）
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static double ObjToMoney(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out double reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 转换为金额（double）
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <param name="errorValue">错误值</param>
        /// <returns></returns>
        public static double ObjToMoney(this object thisValue, double errorValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out double reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return "";
        }

        /// <summary>
        /// 判断字符串不为空
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static bool IsNotEmptyOrNull(this object thisValue)
        {
            return ObjToString(thisValue) != "" && ObjToString(thisValue) != "undefined" &&
                   ObjToString(thisValue) != "null";
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <param name="errorValue">错误值</param>
        /// <returns></returns>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }

        /// <summary>
        /// 判断字符串为空
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object thisValue)
        {
            return thisValue == null || thisValue == DBNull.Value || string.IsNullOrWhiteSpace(thisValue.ToString());
        }

        /// <summary>
        /// 转换为单精度
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static decimal ObjToDecimal(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out decimal reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 转换为单精度
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <param name="errorValue">错误值</param>
        /// <returns></returns>
        public static decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out decimal reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 转换为时间
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }
            else
            {
                //时间戳转为时间
                var seconds = ObjToLong(thisValue);
                if (seconds > 0)
                {
                    var startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
                    reval = startTime.AddSeconds(Convert.ToDouble(thisValue));
                }
            }

            return reval;
        }

        /// <summary>
        /// 转换为时间
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <param name="errorValue">错误值</param>
        /// <returns></returns>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 转换为布尔型
        /// </summary>
        /// <param name="thisValue">待转换值</param>
        /// <returns></returns>
        public static bool ObjToBool(this object thisValue)
        {
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out bool reval))
            {
                return reval;
            }

            return false;
        }

        /// <summary>
        /// 获取当前时间的时间戳
        /// </summary>
        /// <param name="thisValue"></param>
        /// <returns></returns>
        public static string DateToTimeStamp(this DateTime thisValue)
        {
            TimeSpan ts = thisValue - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 转换类型
        /// </summary>
        /// <param name="value">待转换值</param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        public static object ChangeType(this object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string) return Enum.Parse(type, value as string);
                else return Enum.ToObject(type, value);
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;

            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 转换类型列表
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ChangeTypeList(this object value, Type type)
        {
            if (value == null) return default;

            var genericType = typeof(List<>).MakeGenericType(type);
            dynamic list = Activator.CreateInstance(genericType);

            var addMethod = genericType.GetMethod("Add");
            string values = value.ToString();

            if (values != null && values.StartsWith("(") && values.EndsWith(")"))
            {
                string[] splits;
                if (values.Contains("\",\""))
                {
                    splits = values.Remove(values.Length - 2, 2)
                        .Remove(0, 2)
                        .Split("\",\"");
                }
                else
                {
                    splits = values.Remove(0, 1)
                        .Remove(values.Length - 2, 1)
                        .Split(",");
                }

                foreach (var split in splits)
                {
                    var str = split;
                    if (split.StartsWith("\"") && split.EndsWith("\""))
                    {
                        str = split.Remove(0, 1)
                            .Remove(split.Length - 2, 1);
                    }

                    addMethod.Invoke(list, new object[] { ChangeType(str, type) });
                }
            }

            return list;
        }

        /// <summary>
        /// 序化为json
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// 判断集合不存在空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool AnyNoException<T>(this ICollection<T> source)
        {
            if (source == null) return false;

            return source.Any() && source.All(s => s != null);
        }
    }
}
