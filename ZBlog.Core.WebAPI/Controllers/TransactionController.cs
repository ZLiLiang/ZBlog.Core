using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services;

namespace ZBlog.Core.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class TransactionController : ControllerBase
    {
        private readonly IPasswordLibService _passwordLibService;
        private readonly IGuestbookService _guestbookService;
        private readonly IUnitOfWorkManage _unitOfWorkManage;

        public TransactionController(IPasswordLibService passwordLibService, IGuestbookService guestbookService, IUnitOfWorkManage unitOfWorkManage)
        {
            _passwordLibService = passwordLibService;
            _guestbookService = guestbookService;
            _unitOfWorkManage = unitOfWorkManage;
        }

        [HttpGet]
        public async Task<MessageModel<IEnumerable<string>>> Get()
        {
            List<string> returnMsg = new List<string>();
            try
            {
                returnMsg.Add($"Begin Transaction");

                _unitOfWorkManage.BeginTran();
                var passwords = await _passwordLibService.Query(it => it.IsDeleted == false);
                returnMsg.Add($"first time : the count of passwords is :{passwords.Count}");

                returnMsg.Add($"insert a data into the table PasswordLib now.");
                var insertPassword = await _passwordLibService.Add(new Model.Models.PasswordLib
                {
                    IsDeleted = false,
                    AccountName = "aaa",
                    CreateTime = DateTime.Now
                });

                passwords = await _passwordLibService.Query(it => it.IsDeleted == false);
                returnMsg.Add($"second time : the count of passwords is :{passwords.Count}");
                returnMsg.Add($" ");

                var guestbooks = await _guestbookService.Query();
                returnMsg.Add($"first time : the count of guestbooks is :{guestbooks.Count}");

                int ex = 0;
                returnMsg.Add($"There's an exception!!");
                returnMsg.Add($" ");
                int throwEx = 1 / ex;

                var insertGuestbook = await _guestbookService.Add(new Model.Models.Guestbook
                {
                    UserName = "bbb",
                    BlogId = 1,
                    CreateTime = DateTime.Now,
                    IsShow = true
                });

                guestbooks = await _guestbookService.Query();
                returnMsg.Add($"first time : the count of guestbooks is :{guestbooks.Count}");
                returnMsg.Add($" ");

                _unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                _unitOfWorkManage.RollbackTran();
                var passwords = await _passwordLibService.Query();
                returnMsg.Add($"third time : the count of passwords is :{passwords.Count}");

                var guestbooks = await _guestbookService.Query();
                returnMsg.Add($"third time : the count of guestbooks is :{guestbooks.Count}");
            }

            return new MessageModel<IEnumerable<string>>()
            {
                Success = true,
                Msg = "操作完成",
                Response = returnMsg
            };
        }

        [HttpGet("{id}")]
        public async Task<MessageModel<string>> Get(long id)
        {
            return await _guestbookService.TestTranInRepository();
        }

        [HttpGet]
        public async Task<bool> GetTestTranPropagation()
        {
            return await _guestbookService.TestTranPropagation();
        }

        [HttpGet]
        public async Task<bool> GetTestTranPropagationNoTran()
        {
            return await _guestbookService.TestTranPropagationNoTran();
        }

        [HttpGet]
        public async Task<bool> GetTestTranPropagationTran()
        {
            return await _guestbookService.TestTranPropagationTran();
        }

        // POST: api/Transaction
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Transaction/5
        [HttpPut("{id}")]
        public void Put(long id, [FromBody] string value)
        {
        }

        /// <summary>
        /// 测试事务在AOP中的使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<bool> Delete(long id)
        {
            return await _guestbookService.TestTranInRepositoryAOP();
        }
    }
}
