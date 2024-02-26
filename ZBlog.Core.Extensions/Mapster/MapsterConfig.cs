using Mapster;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;

namespace ZBlog.Core.Extensions.Mapster
{
    public class MapsterConfig
    {
        public static TypeAdapterConfig RegisterMappings()
        {
            TypeAdapterConfig config = new TypeAdapterConfig();

            config.ForType<SysUserInfo, SysUserInfoDto>()
                .Map(s => s.uID, d => d.Id)
                .Map(s => s.RIDs, d => d.RIDs)
                .Map(s => s.Addr, d => d.Address)
                .Map(s => s.Age, d => d.Age)
                .Map(s => s.Birth, d => d.Birth)
                .Map(s => s.Status, d => d.Status)
                .Map(s => s.UpdateTime, d => d.UpdateTime)
                .Map(s => s.CreateTime, d => d.CreateTime)
                .Map(s => s.ErrorCount, d => d.ErrorCount)
                .Map(s => s.LastErrTime, d => d.LastErrorTime)
                .Map(s => s.LoginName, d => d.LoginName)
                .Map(s => s.LoginPWD, d => d.LoginPWD)
                .Map(s => s.Remark, d => d.Remark)
                .Map(s => s.RealName, d => d.RealName)
                .Map(s => s.Name, d => d.Name)
                .Map(s => s.IsDelete, d => d.IsDeleted)
                .Map(s => s.RoleNames, d => d.RoleNames);

            config.ForType<SysUserInfoDto, SysUserInfo>()
                .Map(s => s.Id, d => d.uID)
                .Map(s => s.Address, d => d.Addr)
                .Map(s => s.RIDs, d => d.RIDs)
                .Map(s => s.Age, d => d.Age)
                .Map(s => s.Birth, d => d.Birth)
                .Map(s => s.Status, d => d.Status)
                .Map(s => s.UpdateTime, d => d.UpdateTime)
                .Map(s => s.CreateTime, d => d.CreateTime)
                .Map(s => s.ErrorCount, d => d.ErrorCount)
                .Map(s => s.LastErrorTime, d => d.LastErrTime)
                .Map(s => s.LoginName, d => d.LoginName)
                .Map(s => s.LoginPWD, d => d.LoginPWD)
                .Map(s => s.Remark, d => d.Remark)
                .Map(s => s.RealName, d => d.RealName)
                .Map(s => s.Name, d => d.Name)
                .Map(s => s.IsDeleted, d => d.IsDelete)
                .Map(s => s.RoleNames, d => d.RoleNames);

            return config;
        }
    }
}
