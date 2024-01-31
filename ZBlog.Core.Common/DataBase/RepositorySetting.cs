﻿using SqlSugar;
using ZBlog.Core.Model.Models.RootTkey;
using ZBlog.Core.Model.Models.RootTkey.Interface;
using ZBlog.Core.Model.Tenants;

namespace ZBlog.Core.Common.DataBase
{
    public class RepositorySetting
    {
        private static readonly Lazy<IEnumerable<Type>> AllEntitys = new(() =>
        {
            return typeof(BaseEntity).Assembly
                .GetTypes()
                .Where(it => it.IsClass && !it.IsAbstract && it.IsSubclassOf(typeof(BaseEntity)))
                .Where(it => it.FullName != null && it.FullName.StartsWith("Blog.Core.Model.Models"));
        });

        public static IEnumerable<Type> Entitys => AllEntitys.Value;

        /// <summary>
        /// 配置实体软删除过滤器<br/>
        /// 统一过滤 软删除 无需自己写条件
        /// </summary>
        public static void SetDeletedEntityFilter(SqlSugarScopeProvider db)
        {
            db.QueryFilter.AddTableFilter<IDeleteFilter>(it => it.IsDeleted == false);
        }

        /// <summary>
        /// 配置租户
        /// </summary>
        public static void SetTenantEntityFilter(SqlSugarScopeProvider db)
        {
            if (App.User is not { ID: > 0, TenantId: > 0 })
            {
                return;
            }

            //多租户 单表
            db.QueryFilter.AddTableFilter<ITenantEntity>(it => it.TenantId == App.User.TenantId || it.TenantId == 0);

            //多租户 多表
            db.SetTenantTable(App.User.TenantId.ToString());
        }
    }
}
