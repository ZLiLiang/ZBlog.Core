using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.Common.Static
{
    public static class StaticPayInfo
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public readonly static string MERCHANTID = AppSettings.App(new string[] { "PayInfo", "MERCHANTID" }).ObjToString();
        /// <summary>
        /// 柜台号
        /// </summary>
        public readonly static string POSID = AppSettings.App(new string[] { "PayInfo", "POSID" }).ObjToString();
        /// <summary>
        /// 分行号
        /// </summary>
        public readonly static string BRANCHID = AppSettings.App(new string[] { "PayInfo", "BRANCHID" }).ObjToString();
        /// <summary>
        /// 公钥
        /// </summary>
        public readonly static string pubKey = AppSettings.App(new string[] { "PayInfo", "pubKey" }).ObjToString();
        /// <summary>
        /// 操作员号
        /// </summary>
        public readonly static string USER_ID = AppSettings.App(new string[] { "PayInfo", "USER_ID" }).ObjToString();
        /// <summary>
        /// 密码
        /// </summary>
        public readonly static string PASSWORD = AppSettings.App(new string[] { "PayInfo", "PASSWORD" }).ObjToString();
        /// <summary>
        /// 外联平台通讯地址
        /// </summary>
        public readonly static string OutAddress = AppSettings.App(new string[] { "PayInfo", "OutAddress" }).ObjToString();
    }
}
