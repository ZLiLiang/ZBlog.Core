namespace ZBlog.Core.Common.Extensions
{
    public static class UntilExtension
    {
        public static void AddOrModify<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
        {
            if (dic.TryGetValue(key, out _))
                dic[key] = value;
            else
                dic.Add(key, value);
        }
    }
}
