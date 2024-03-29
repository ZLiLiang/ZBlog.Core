﻿using SqlSugar;
using ZBlog.Core.Model.Models.Tenant;

namespace ZBlog.Core.Common.Seed.SeedData
{
    public class SubBusinessDataSeedData : IEntitySeedData<SubLibraryBusinessTable>
    {
        public async Task CustomizeSeedData(ISqlSugarClient db)
        {
            //初始化分库数据
            //只是用于测试
            if (db.CurrentConnectionConfig.ConfigId == "Tenant_3")
            {
                if (!await db.Queryable<SubLibraryBusinessTable>().AnyAsync())
                {
                    await db.Insertable(new List<SubLibraryBusinessTable>
                    {
                        new()
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            Name = "王五业务数据1",
                            Amount = 100,
                        },
                        new()
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            Name = "王五业务数据2",
                            Amount = 1000,
                        },
                    }).ExecuteReturnSnowflakeIdListAsync();
                }
            }
            else if (db.CurrentConnectionConfig.ConfigId == "Tenant_4")
            {
                if (!await db.Queryable<SubLibraryBusinessTable>().AnyAsync())
                {
                    await db.Insertable(new List<SubLibraryBusinessTable>
                    {
                        new()
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            Name = "赵六业务数据1",
                            Amount = 50,
                        },
                        new()
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            Name = "赵六业务数据2",
                            Amount = 60,
                        },
                    }).ExecuteReturnSnowflakeIdListAsync();
                }
            }

            await Task.Delay(1);
        }

        public IEnumerable<SubLibraryBusinessTable> InitSeedData()
        {
            return default;
        }

        public IEnumerable<SubLibraryBusinessTable> SeedData()
        {
            return default;
        }
    }
}
