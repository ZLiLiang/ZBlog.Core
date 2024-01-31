﻿using SqlSugar;
using ZBlog.Core.Model.Models.Tenant;

namespace ZBlog.Core.Common.Seed.SeedData
{
    public class MultiBusinessDataSeedData : IEntitySeedData<MultiBusinessTable>
    {
        public IEnumerable<MultiBusinessTable> InitSeedData()
        {
            return new List<MultiBusinessTable>()
            {
                new()
                {
                    Id = 1001,
                    Name = "业务数据1",
                    Amount = 100,
                },
                new()
                {
                    Id = 1002,
                    Name = "业务数据2",
                    Amount = 1000,
                },
            };
        }

        public IEnumerable<MultiBusinessTable> SeedData()
        {
            return default;
        }

        public Task CustomizeSeedData(ISqlSugarClient db)
        {
            return Task.CompletedTask;
        }
    }
}
