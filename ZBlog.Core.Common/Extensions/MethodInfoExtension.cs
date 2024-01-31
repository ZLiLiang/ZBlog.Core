using System.Reflection;

namespace ZBlog.Core.Common.Extensions
{
    public static class MethodInfoExtension
    {
        public static string GetFullName(this MethodInfo method)
        {
            if (method.DeclaringType == null)
                return $@"{method.Name}";

            return $"{method.DeclaringType.FullName}.{method.Name}";
        }
    }
}
