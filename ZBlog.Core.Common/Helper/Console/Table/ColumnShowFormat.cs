namespace ZBlog.Core.Common.Helper.Console.Table
{
    /// <summary>
    /// 列显示格式信息
    /// </summary>
    public class ColumnShowFormat
    {
        public ColumnShowFormat(int index, Alignment alignment, int strLength)
        {
            Index = index;
            Alignment = alignment;
            StrLength = strLength;
        }

        /// <summary>
        /// 索引，第几列数据
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 对其方式
        /// </summary>
        public Alignment Alignment { get; set; }

        /// <summary>
        /// 一列字符串长度
        /// </summary>
        public int StrLength { get; set; }
    }

    /// <summary>
    /// 对其方式
    /// </summary>
    public enum Alignment
    {
        Left,
        Right
    }
}
