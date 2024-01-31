using System.Text.RegularExpressions;

namespace ZBlog.Core.Common.Helper
{
    public static class HtmlHelper
    {
        #region 去除富文本中的HTML标签

        /// <summary>
        /// 去除富文本中的HTML标签
        /// </summary>
        /// <param name="html">html文本</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = Regex.Replace(html, "<[^>]+>", "");
            strText = Regex.Replace(strText, "&[^;]+;", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

        #endregion
    }
}
