namespace ZBlog.Core.Serilog.Es.Formatters
{
    public class LogConfigRootDto
    {
        /// <summary>
        /// tcp日志的host地址
        /// </summary>
        public string TcpAddressHost { set; get; }

        /// <summary>
        /// tcp日志的port地址
        /// </summary>
        public int TcpAddressPort { set; get; }

        public List<ConfigsInfo> ConfigsInfos { get; set; }
    }

    public class ConfigsInfo
    {
        public string FiedName { get; set; }
        public string FiedValue { get; set; }
    }
}
