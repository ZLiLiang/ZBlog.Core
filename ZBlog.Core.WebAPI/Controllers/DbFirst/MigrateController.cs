using Com.Ctrip.Framework.Apollo.Enums;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;

namespace ZBlog.Core.WebAPI.Controllers.DbFirst
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MigrateController : ControllerBase
    {
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        private readonly IRoleModulePermissionService _roleModulePermissionService;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IModuleService _moduleService;
        private readonly IDepartmentService _departmentService;
        private readonly ISysUserInfoService _sysUserInfoService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MigrateController(IUnitOfWorkManage unitOfWorkManage, IRoleModulePermissionService roleModulePermissionService, IUserRoleService userRoleService, IRoleService roleService, IPermissionService permissionService, IModuleService moduleService, IDepartmentService departmentService, ISysUserInfoService sysUserInfoService, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _roleModulePermissionService = roleModulePermissionService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _permissionService = permissionService;
            _moduleService = moduleService;
            _departmentService = departmentService;
            _sysUserInfoService = sysUserInfoService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// 获取权限部分Map数据（从库）
        /// 迁移到新库（主库）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> DataMigrateFromOldToNew()
        {
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };
            var filterPermissionId = 122;
            if (_webHostEnvironment.IsDevelopment())
            {
                try
                {
                    var apiList = await _moduleService.Query(it => it.IsDeleted == false);
                    var permissionsAllList = await _permissionService.Query(it => it.IsDeleted == false);
                    var permissions = permissionsAllList.Where(it => it.Pid == 0).ToList();
                    var rmps = await _roleModulePermissionService.GetRMPMaps();
                    List<PM> pms = new();

                    // 当然，你可以做个where查询
                    rmps = rmps.Where(it => it.PermissionId >= filterPermissionId).ToList();

                    InitPermissionTree(permissions, permissionsAllList, apiList);

                    var actionPermissionIds = permissionsAllList.Where(it => it.Id >= filterPermissionId)
                        .Select(it => it.Id)
                        .ToList();
                    List<long> filterPermissionIds = new();
                    FilterPermissionTree(permissionsAllList, actionPermissionIds, filterPermissionIds);
                    permissions = permissions.Where(it => filterPermissionIds.Contains(it.Id))
                        .ToList();

                    // 开启事务，保证数据一致性
                    _unitOfWorkManage.BeginTran();

                    // 注意信息的完整性，不要重复添加，确保主库没有要添加的数据

                    // 1、保持菜单和接口
                    await SavePermissionTreeAsync(permissions, pms);

                    long rid = 0;
                    long pid = 0;
                    long mid = 0;
                    long rpmid = 0;

                    // 2、保存关系表
                    foreach (var item in rmps)
                    {
                        // 角色信息，防止重复添加，做了判断
                        if (item.Role != null)
                        {
                            var isExit = (await _roleService.Query(it => it.Name == item.Role.Name && it.IsDeleted == false)).FirstOrDefault();
                            if (isExit == null)
                            {
                                rid = await _roleService.Add(item.Role);
                                Console.WriteLine($"Role Added:{item.Role.Name}");
                            }
                            else
                            {
                                rid = isExit.Id;
                            }
                        }

                        pid = (pms.FirstOrDefault(d => d.PidOld == item.PermissionId)?.PidNew).ObjToLong();
                        mid = (pms.FirstOrDefault(d => d.MidOld == item.ModuleId)?.MidNew).ObjToLong();

                        // 关系
                        if (rid > 0 && pid > 0)
                        {
                            rpmid = await _roleModulePermissionService.Add(new RoleModulePermission
                            {
                                IsDeleted = false,
                                CreateTime = DateTime.Now,
                                ModifyTime = DateTime.Now,
                                ModuleId = mid,
                                PermissionId = pid,
                                RoleId = rid
                            });
                            Console.WriteLine($"RMP Added:{rpmid}");
                        }
                    }

                    _unitOfWorkManage.CommitTran();

                    data.Success = true;
                    data.Msg = "导入成功！";
                }
                catch (Exception)
                {
                    _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Success = true;
                data.Msg = "当前不处于开发模式，代码生成不可用！";
            }

            return data;
        }

        /// <summary>
        /// 权限数据库导出tsv
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> SaveDataToTsvAsync()
        {
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };

            if (_webHostEnvironment.IsDevelopment())
            {
                JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };

                // 取出数据，序列化，自己可以处理判空
                var sysUserInfoJson = JsonConvert.SerializeObject(await _sysUserInfoService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "SysUserInfo.tsv"), sysUserInfoJson, Encoding.UTF8);

                var departmentJson = JsonConvert.SerializeObject(await _departmentService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "Department.tsv"), departmentJson, Encoding.UTF8);

                var rolesJson = JsonConvert.SerializeObject(await _roleService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "Role.tsv"), rolesJson, Encoding.UTF8);

                var userRoleJson = JsonConvert.SerializeObject(await _userRoleService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "UserRole.tsv"), userRoleJson, Encoding.UTF8);


                var permissionsJson = JsonConvert.SerializeObject(await _permissionService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "Permission.tsv"), permissionsJson, Encoding.UTF8);


                var modulesJson = JsonConvert.SerializeObject(await _moduleService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "Modules.tsv"), modulesJson, Encoding.UTF8);


                var rmpsJson = JsonConvert.SerializeObject(await _roleModulePermissionService.Query(d => d.IsDeleted == false), microsoftDateFormatSettings);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.json", "RoleModulePermission.tsv"), rmpsJson, Encoding.UTF8);

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

        /// <summary>
        /// 权限数据库导出excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> SaveDataToExcelAsync()
        {
            var data = new MessageModel<string>
            {
                Success = true,
                Msg = ""
            };

            if (_webHostEnvironment.IsDevelopment())
            {
                JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };

                // 取出数据，序列化，自己可以处理判空
                IExporter exporter = new ExcelExporter();
                var sysUserInfoList = await _sysUserInfoService.Query(d => d.IsDeleted == false);
                var sysUserInfoResult = await exporter.ExportAsByteArray(sysUserInfoList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "SysUserInfo.xlsx"), sysUserInfoResult);

                var departmentList = await _departmentService.Query(d => d.IsDeleted == false);
                var departmentResult = await exporter.ExportAsByteArray(departmentList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "Department.xlsx"), departmentResult);

                var roleList = await _roleService.Query(d => d.IsDeleted == false);
                var roleResult = await exporter.ExportAsByteArray(roleList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "Role.xlsx"), roleResult);

                var userRoleList = await _userRoleService.Query(d => d.IsDeleted == false);
                var userRoleResult = await exporter.ExportAsByteArray(userRoleList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "UserRole.xlsx"), userRoleResult);

                var permissionList = await _permissionService.Query(d => d.IsDeleted == false);
                var permissionResult = await exporter.ExportAsByteArray(permissionList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "Permission.xlsx"), permissionResult);

                var modulesList = await _moduleService.Query(d => d.IsDeleted == false);
                var modulesResult = await exporter.ExportAsByteArray(modulesList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "Modules.xlsx"), modulesResult);

                var roleModulePermissionList = await _roleModulePermissionService.Query(d => d.IsDeleted == false);
                var roleModulePermissionResult = await exporter.ExportAsByteArray(roleModulePermissionList);
                FileHelper.WriteFile(Path.Combine(_webHostEnvironment.WebRootPath, "ZBlogCore.Data.excel", "RoleModulePermission.xlsx"), roleModulePermissionResult);

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

        private void InitPermissionTree(List<Permission> permissionsTree, List<Permission> all, List<Modules> apis)
        {
            foreach (var item in permissionsTree)
            {
                item.Children = all.Where(d => d.Pid == item.Id).ToList();
                item.Module = apis.FirstOrDefault(d => d.Id == item.Mid);
                InitPermissionTree(item.Children, all, apis);
            }
        }

        private void FilterPermissionTree(List<Permission> permissionsAll, List<long> actionPermissionId, List<long> filterPermissionIds)
        {
            actionPermissionId = actionPermissionId.Distinct().ToList();
            var doneIds = permissionsAll.Where(d => actionPermissionId.Contains(d.Id) && d.Pid == 0)
                .Select(d => d.Id)
                .ToList();
            filterPermissionIds.AddRange(doneIds);

            var hasDoIds = permissionsAll.Where(d => actionPermissionId.Contains(d.Id) && d.Pid != 0)
                .Select(d => d.Pid)
                .ToList();
            if (hasDoIds.Any())
            {
                FilterPermissionTree(permissionsAll, hasDoIds, filterPermissionIds);
            }
        }

        private async Task SavePermissionTreeAsync(List<Permission> permissionsTree, List<PM> pms, long permissionId = 0)
        {
            var parendId = permissionId;

            foreach (var item in permissionsTree)
            {
                // 保留原始主键id
                var pm = new PM
                {
                    PidOld = item.Id,
                    MidOld = (item.Module?.Id).ObjToLong()
                };

                long mid = 0;
                // 接口
                if (item.Module != null)
                {
                    var moduleModel = (await _moduleService.Query(d => d.LinkUrl == item.Module.LinkUrl)).FirstOrDefault();
                    if (moduleModel != null)
                    {
                        mid = moduleModel.Id;
                    }
                    else
                    {
                        mid = await _moduleService.Add(item.Module);
                    }
                    pm.MidNew = mid;
                    Console.WriteLine($"Moudle Added:{item.Module.Name}");
                }

                // 菜单
                if (item != null)
                {
                    var permissionModel = (await _permissionService.Query(d => d.Name == item.Name && d.Pid == item.Pid && d.Mid == item.Mid)).FirstOrDefault();
                    item.Pid = parendId;
                    item.Mid = mid;
                    if (permissionModel != null)
                    {
                        permissionId = permissionModel.Id;
                    }
                    else
                    {
                        permissionId = await _permissionService.Add(item);
                    }

                    pm.PidNew = permissionId;
                    Console.WriteLine($"Permission Added:{item.Name}");
                }
                pms.Add(pm);

                await SavePermissionTreeAsync(item.Children, pms, permissionId);
            }
        }
    }

    public class PM
    {
        public long PidOld { get; set; }
        public long MidOld { get; set; }
        public long PidNew { get; set; }
        public long MidNew { get; set; }
    }
}
