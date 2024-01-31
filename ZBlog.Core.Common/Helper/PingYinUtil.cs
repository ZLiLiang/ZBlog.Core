﻿using Microsoft.International.Converters.PinYinConverter;
using System.Text.RegularExpressions;

namespace ZBlog.Core.Common.Helper
{
    /// <summary>
    /// 汉字转换拼音
    /// </summary>
    public static class PingYinUtil
    {
        private static Dictionary<int, List<string>> GetTotalPingYinDictionary(string text)
        {
            var chars = text.ToCharArray();

            // 记录每个汉字的全拼
            Dictionary<int, List<string>> totalPingYinList = new Dictionary<int, List<string>>();

            for (int i = 0; i < chars.Length; i++)
            {
                var pinyinList = new List<string>();

                // 是否是有效的汉字
                if (ChineseChar.IsValidChar(chars[i]))
                {
                    ChineseChar chineseChar = new ChineseChar(chars[i]);
                    pinyinList = chineseChar.Pinyins
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .ToList();
                }
                else
                {
                    pinyinList.Add(chars[i].ToString());
                }

                // 去除声调，转小写
                pinyinList = pinyinList.ConvertAll(p => Regex.Replace(p, @"\d", "").ToLower());

                // 去重
                pinyinList = pinyinList.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToList();
                if (pinyinList.Any())
                {
                    totalPingYinList[i] = pinyinList;
                }
            }

            return totalPingYinList;
        }

        /// <summary>
        /// 获取汉语拼音全拼
        /// </summary>
        /// <param name="text">The string.</param>
        /// <returns></returns>
        public static List<string> GetTotalPingYin(this string text)
        {
            var result = new List<string>();
            foreach (var pys in GetTotalPingYinDictionary(text))
            {
                var items = pys.Value;
                if (result.Count > 0)
                {
                    //全拼循环匹配
                    var newTotalPingYinList = new List<string>();
                    foreach (var totalPingYin in result)
                    {
                        newTotalPingYinList.AddRange(items.Select(item => totalPingYin + item));
                    }
                    newTotalPingYinList = newTotalPingYinList.Distinct().ToList();
                    result = newTotalPingYinList;
                }
                else
                {
                    result = items;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取汉语拼音首字母
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> GetFirstPingYin(this string text)
        {
            var result = new List<string>();
            foreach (var pys in GetTotalPingYinDictionary(text))
            {
                var items = pys.Value;
                if (result.Count > 0)
                {
                    //首字母循环匹配
                    var newFirstPingYinList = new List<string>();
                    foreach (var firstPingYin in result)
                    {
                        newFirstPingYinList.AddRange(items.Select(item => firstPingYin + item.Substring(0, 1)));
                    }
                    newFirstPingYinList = newFirstPingYinList.Distinct().ToList();
                    result = newFirstPingYinList;
                }
                else
                {
                    result = items.ConvertAll(p => p.Substring(0, 1)).Distinct().ToList();
                }
            }
            return result;
        }
    }
}
