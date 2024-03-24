using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.IServices;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.UnitOfWorks;
using System.Linq.Expressions;
using ZBlog.Core.Common.Helper;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 分表demo
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Permissions.Name)]
    public class SplitDemoController : ControllerBase
    {
        private readonly ISplitDemoService _splitDemoService;
        private readonly IUnitOfWorkManage _unitOfWorkManage;

        public SplitDemoController(ISplitDemoService splitDemoService, IUnitOfWorkManage unitOfWorkManage)
        {
            _splitDemoService = splitDemoService;
            _unitOfWorkManage = unitOfWorkManage;
        }

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="key"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<PageModel<SplitDemo>>> Get(DateTime beginTime,
                                                                  DateTime endTime,
                                                                  int page = 1,
                                                                  string key = "",
                                                                  int pageSize = 10)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            Expression<Func<SplitDemo, bool>> whereExpression = a => (a.Name != null && a.Name.Contains(key));
            var data = await _splitDemoService.QueryPageSplit(whereExpression, beginTime, endTime, page, pageSize, " Id desc ");

            return MessageModel<PageModel<SplitDemo>>.Message(data.DataCount >= 0, "获取成功", data);
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<SplitDemo>> GetById(long id)
        {
            var data = new MessageModel<string>();
            var model = await _splitDemoService.QueryByIdSplit(id);
            if (model != null)
            {
                return MessageModel<SplitDemo>.SUCCESS("获取成功", model);
            }
            else
            {
                return MessageModel<SplitDemo>.FAIL("获取失败");
            }
        }

        /// <summary>
        /// 添加一条测试数据
        /// </summary>
        /// <param name="splitDemo"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MessageModel<string>> Post([FromBody] SplitDemo splitDemo)
        {
            var data = new MessageModel<string>();
            var id = await _splitDemoService.AddSplit(splitDemo);
            data.Success = (id == null ? false : true);
            try
            {
                if (data.Success)
                {
                    data.Response = id.FirstOrDefault().ToString();
                    data.Msg = "添加成功";
                }
                else
                {
                    data.Msg = "添加失败";
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }

            return data;
        }

        /// <summary>
        /// 修改一条测试数据
        /// </summary>
        /// <param name="splitDemo"></param>
        /// <returns></returns>
        [HttpPut]
        [AllowAnonymous]
        public async Task<MessageModel<string>> Put([FromBody] SplitDemo splitDemo)
        {
            var data = new MessageModel<string>();
            if (splitDemo != null && splitDemo.Id > 0)
            {
                _unitOfWorkManage.BeginTran();
                data.Success = await _splitDemoService.UpdateSplit(splitDemo, splitDemo.CreateTime);
                try
                {
                    if (data.Success)
                    {
                        data.Msg = "修改成功";
                        data.Response = splitDemo?.Id.ObjToString();
                    }
                    else
                    {
                        data.Msg = "修改失败";
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }

            return data;
        }

        /// <summary>
        /// 根据id删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [AllowAnonymous]
        public async Task<MessageModel<string>> Delete(long id)
        {
            var data = new MessageModel<string>();
            var model = await _splitDemoService.QueryByIdSplit(id);

            if (model != null)
            {
                _unitOfWorkManage.BeginTran();
                data.Success = await _splitDemoService.DeleteSplit(model, model.CreateTime);
                try
                {
                    data.Response = id.ObjToString();
                    if (data.Success)
                        data.Msg = "删除成功";
                    else
                        data.Msg = "删除失败";
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    if (data.Success)
                        _unitOfWorkManage.CommitTran();
                    else
                        _unitOfWorkManage.RollbackTran();
                }
            }
            else
            {
                data.Msg = "不存在";
            }

            return data;
        }
    }
}
