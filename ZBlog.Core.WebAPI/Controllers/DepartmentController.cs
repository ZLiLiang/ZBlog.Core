using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using System.Linq.Expressions;
using ZBlog.Core.Services;
using ZBlog.Core.Common.Helper;
using Newtonsoft.Json;
using Com.Ctrip.Framework.Apollo.Enums;
using System.Text;

namespace ZBlog.Core.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class DepartmentController : BaseApiController
    {
        private readonly IDepartmentService _departmentService;
        private readonly IWebHostEnvironment _environment;

        public DepartmentController(IDepartmentService departmentService, IWebHostEnvironment environment)
        {
            _departmentService = departmentService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<Department>>> Get(int page = 1, string key = "", int intPageSize = 50)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            Expression<Func<Department, bool>> whereExpression = a => true;

            return new MessageModel<PageModel<Department>>
            {
                Msg = "获取成功",
                Success = true,
                Response = await _departmentService.QueryPage(whereExpression, page, intPageSize)
            };
        }

        [HttpGet("{id}")]
        public async Task<MessageModel<Department>> Get(string id)
        {
            return new MessageModel<Department>()
            {
                Msg = "获取成功",
                Success = true,
                Response = await _departmentService.QueryById(id)
            };
        }

        /// <summary>
        /// 查询树形 Table
        /// </summary>
        /// <param name="father">父节点</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        public async Task<MessageModel<List<Department>>> GetTreeTable(long father = 0, string key = "")
        {
            List<Department> departments = new List<Department>();
            var departmentList = await _departmentService.Query(d => d.IsDeleted == false);
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }

            if (key != "")
            {
                departments = departmentList.Where(a => a.Name.Contains(key))
                    .OrderBy(a => a.OrderSort)
                    .ToList();
            }
            else
            {
                departments = departmentList.Where(a => a.Pid == father)
                    .OrderBy(a => a.OrderSort)
                    .ToList();
            }

            foreach (var item in departments)
            {
                List<long> pidarr = new() { };
                var parent = departmentList.FirstOrDefault(d => d.Id == item.Pid);

                while (parent != null)
                {
                    pidarr.Add(parent.Id);
                    parent = departmentList.FirstOrDefault(d => d.Id == parent.Pid);
                }

                pidarr.Reverse();
                pidarr.Insert(0, 0);
                item.PidArr = pidarr;

                item.HasChildren = departmentList.Where(d => d.Pid == item.Id).Any();
            }

            return Success(departments, "获取成功");
        }

        /// <summary>
        /// 获取部门树
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<DepartmentTree>> GetDepartmentTree(long pid = 0)
        {
            var departments = await _departmentService.Query(d => d.IsDeleted == false);
            var departmentTrees = departments.Where(it => it.IsDeleted == false)
                .OrderBy(it => it.Id)
                .Select(it => new DepartmentTree
                {
                    Value = it.Id,
                    Label = it.Name,
                    PId = it.Pid,
                    Order = it.OrderSort
                })
                .ToList();
            DepartmentTree rootTree = new DepartmentTree
            {
                Value = 0,
                PId = 0,
                Label = "根节点"
            };

            departmentTrees = departmentTrees.OrderBy(d => d.Order)
                .ToList();

            RecursionHelper.LoopToAppendChildren(departmentTrees, rootTree, pid);

            return Success(rootTree, "获取成功");
        }

        [HttpPost]
        public async Task<MessageModel<string>> Post([FromBody] Department request)
        {
            var data = new MessageModel<string>();
            var id = await _departmentService.Add(request);

            data.Success = id > 0;
            if (data.Success)
            {
                data.Response = id.ObjToString();
                data.Msg = "添加成功";
            }

            return data;
        }

        [HttpPut]
        public async Task<MessageModel<string>> Put([FromBody] Department request)
        {
            var data = new MessageModel<string>();
            data.Success = await _departmentService.Update(request);
            if (data.Success)
            {
                data.Msg = "更新成功";
                data.Response = request?.Id.ObjToString();
            }

            return data;
        }

        [HttpDelete]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            var model = await _departmentService.QueryById(id);
            model.IsDeleted = true;
            data.Success = await _departmentService.Update(model);
            if (data.Success)
            {
                data.Msg = "删除成功";
                data.Response = model?.Id.ObjToString();
            }

            return data;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<string>> SaveDataToTsv()
        {
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            if (_environment.IsDevelopment())
            {
                JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };

                var rolesJson = JsonConvert.SerializeObject(await _departmentService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_environment.WebRootPath, "ZBlogCore.Data.json", "Department_New.tsv"), rolesJson, Encoding.UTF8);

                data.Success = true;
                data.Msg = "生成成功！";
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
