using SqlSugar;

namespace ZBlog.Core.Model.IDS4DbModels
{
    /// <summary>
    /// 以下model 来自ids4项目，多库模式，为了调取ids4数据
    /// 用户表
    /// </summary>
    [SugarTable("AspNetUsers", "用户表")]  //('数据库表名'，'数据库表备注')
    [TenantAttribute("WMBLOG_MYSQL_2")] //('代表是哪个数据库，名字是appsettings.json 的 ConnId')
    public class ApplicationUser
    {
        public string LoginName { get; set; }

        public string RealName { get; set; }

        public int Sex { get; set; } = 0;

        public int Age { get; set; }

        public DateTime Birth { get; set; } = DateTime.Now;

        public string Addr { get; set; }

        public bool TdIsDelete { get; set; }

    }
}
