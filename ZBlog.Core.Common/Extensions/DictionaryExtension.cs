﻿namespace ZBlog.Core.Common.Extensions
{
    public static class DictionaryExtension
    {
        public static void TryAdd<TKey, TValue>(this IDictionary<TKey, List<TValue>> dic, TKey key, TValue value)
        {
            if (dic.TryGetValue(key, out var old))
            {
                old.Add(value);
            }
            else
            {
                dic.Add(key, new List<TValue> { value });
            }
        }
    }
}
