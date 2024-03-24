using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZBlog.Core.Common.DB;
using ZBlog.Core.Common.Seed;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers.DbFirst
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DbFirstController : ControllerBase
    {
        private readonly SqlSugarScope _sqlSugarClient;
        private readonly IWebHostEnvironment _environment;

        public DbFirstController(ISqlSugarClient sqlSugarClient, IWebHostEnvironment environment)
        {
            this._sqlSugarClient = sqlSugarClient as SqlSugarScope;
            this._environment = environment;
        }

        /// <summary>
        /// 获取 整体框架 文件(主库)(一般可用第一次生成)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<string> GetFrameFiles()
        {
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            data.Response += @"file path is:C:\my-file\}";
            var isMuti = BaseDBConfig.IsMulti;
            if (_environment.IsDevelopment())
            {
                data.Response += $"Controller层生成：{FrameSeed.CreateControllers(_sqlSugarClient)} || ";

                BaseDBConfig.ValidConfig.ForEach(m =>
                {
                    _sqlSugarClient.ChangeDatabase(m.ConfigId.ToString().ToLower());
                    data.Response += $"库{m.ConfigId}-Model层生成：{FrameSeed.CreateModels(_sqlSugarClient, m.ConfigId.ToString(), isMuti)} || ";
                    data.Response += $"库{m.ConfigId}-IRepositorys层生成：{FrameSeed.CreateIRepositorys(_sqlSugarClient, m.ConfigId.ToString(), isMuti)} || ";
                    data.Response += $"库{m.ConfigId}-IServices层生成：{FrameSeed.CreateIServices(_sqlSugarClient, m.ConfigId.ToString(), isMuti)} || ";
                    data.Response += $"库{m.ConfigId}-Repository层生成：{FrameSeed.CreateRepository(_sqlSugarClient, m.ConfigId.ToString(), isMuti)} || ";
                    data.Response += $"库{m.ConfigId}-Services层生成：{FrameSeed.CreateServices(_sqlSugarClient, m.ConfigId.ToString(), isMuti)} || ";
                });

                // 切回主库
                _sqlSugarClient.ChangeDatabase(MainDb.CurrentDbConnId.ToLower());
            }
            else
            {
                data.Success = false;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }

        /// <summary>
        /// 获取仓储层和服务层(需指定表名和数据库)
        /// </summary>
        /// <param name="tableNames">需要生成的表名</param>
        /// <param name="ConnID">数据库链接名称</param>
        /// <returns></returns>
        [HttpPost]
        public MessageModel<string> GetFrameFilesByTableNames([FromBody] string[] tableNames, [FromQuery] string ConnID = null)
        {
            ConnID = ConnID == null ? MainDb.CurrentDbConnId.ToLower() : ConnID;

            var isMuti = BaseDBConfig.IsMulti;
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            if (_environment.IsDevelopment())
            {
                data.Response += $"库{ConnID}-IRepositorys层生成：{FrameSeed.CreateIRepositorys(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-IServices层生成：{FrameSeed.CreateIServices(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-Repository层生成：{FrameSeed.CreateRepository(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-Services层生成：{FrameSeed.CreateServices(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
            }
            else
            {
                data.Success = false;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }

        /// <summary>
        /// 获取实体(需指定表名和数据库)
        /// </summary>
        /// <param name="tableNames">需要生成的表名</param>
        /// <param name="ConnID">数据库链接名称</param>
        /// <returns></returns>
        [HttpPost]
        public MessageModel<string> GetFrameFilesByTableNamesForEntity([FromBody] string[] tableNames, [FromQuery] string ConnID = null)
        {
            ConnID = ConnID == null ? MainDb.CurrentDbConnId.ToLower().ToUpper() : ConnID;

            var isMuti = BaseDBConfig.IsMulti;
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            if (_environment.IsDevelopment())
            {
                data.Response += $"库{ConnID}-Models层生成：{FrameSeed.CreateModels(_sqlSugarClient, ConnID, isMuti, tableNames)}";
            }
            else
            {
                data.Success = false;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }

        /// <summary>
        /// 获取控制器(需指定表名和数据库)
        /// </summary>
        /// <param name="tableNames">需要生成的表名</param>
        /// <param name="ConnID">数据库链接名称</param>
        /// <returns></returns>
        [HttpPost]
        public MessageModel<string> GetFrameFilesByTableNamesForController([FromBody] string[] tableNames, [FromQuery] string ConnID = null)
        {
            ConnID = ConnID == null ? MainDb.CurrentDbConnId.ToLower() : ConnID;

            var isMuti = BaseDBConfig.IsMulti;
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            if (_environment.IsDevelopment())
            {
                data.Response += $"库{ConnID}-Controllers层生成：{FrameSeed.CreateControllers(_sqlSugarClient, ConnID, isMuti, tableNames)}";
            }
            else
            {
                data.Success = false;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }

        /// <summary>
        /// DbFrist 根据数据库表名 生成整体框架,包含Model层(一般可用第一次生成)
        /// </summary>
        /// <param name="tableNames">需要生成的表名</param>
        /// <param name="ConnID">数据库链接名称</param>
        /// <returns></returns>
        [HttpPost]
        public MessageModel<string> GetAllFrameFilesByTableNames([FromBody] string[] tableNames, [FromQuery] string ConnID = null)
        {
            ConnID = ConnID == null ? MainDb.CurrentDbConnId.ToLower() : ConnID;

            var isMuti = BaseDBConfig.IsMulti;
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            if (_environment.IsDevelopment())
            {
                _sqlSugarClient.ChangeDatabase(ConnID.ToLower());
                data.Response += $"Controller层生成：{FrameSeed.CreateControllers(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-Model层生成：{FrameSeed.CreateModels(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-IRepositorys层生成：{FrameSeed.CreateIRepositorys(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-IServices层生成：{FrameSeed.CreateIServices(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-Repository层生成：{FrameSeed.CreateRepository(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";
                data.Response += $"库{ConnID}-Services层生成：{FrameSeed.CreateServices(_sqlSugarClient, ConnID, isMuti, tableNames)} || ";

                // 切回主库
                _sqlSugarClient.ChangeDatabase(MainDb.CurrentDbConnId.ToLower());
            }
            else
            {
                data.Success = false;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }
    }
}
