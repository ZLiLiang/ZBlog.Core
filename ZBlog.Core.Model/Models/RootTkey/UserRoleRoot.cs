namespace ZBlog.Core.Model.Models.RootTkey
{
    /// <summary>
    /// 用户跟角色关联表
    /// 父类
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    public class UserRoleRoot<Tkey> : RootEntityTkey<Tkey> where Tkey : IEquatable<Tkey>
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Tkey UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public Tkey RoleId { get; set; }
    }
}
