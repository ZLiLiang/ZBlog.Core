using ZBlog.Core.Common.Option.Core;

namespace ZBlog.Core.Common.Option
{
    public class SeqOptions : IConfigurableOptions
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        public string ApiKey { get; set; }
    }
}
