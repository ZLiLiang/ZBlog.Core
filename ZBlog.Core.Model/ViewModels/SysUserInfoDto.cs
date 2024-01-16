using ZBlog.Core.Model.ViewModels.RootTKey;

namespace ZBlog.Core.Model.ViewModels
{
    public class SysUserInfoDto : SysUserInfoDtoRoot<long>
    {
        public string LoginName { get; set; }
        public string LoginPWD { get; set; }
        public string RealName { get; set; }
        public int Status { get; set; }
        public long DepartmentId { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UpdateTime { get; set; } = DateTime.Now;
        public DateTime LastErrTime { get; set; } = DateTime.Now;
        public int ErrorCount { get; set; }
        public string Name { get; set; }
        public int Sex { get; set; } = 0;
        public int Age { get; set; }
        public DateTime Birth { get; set; } = DateTime.Now;
        public string Addr { get; set; }
        public bool IsDelete { get; set; }
        public List<string> RoleNames { get; set; }
        public List<long> Dids { get; set; }
        public string DepartmentName { get; set; }
    }
}
