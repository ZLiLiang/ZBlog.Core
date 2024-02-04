using ZBlog.Core.Common.CustomAttribute;
using ZBlog.Core.Common.DataBase;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Repository.Base;
using ZBlog.Core.Repository.UnitOfWorks;
using ZBlog.Core.Services.Base;

namespace ZBlog.Core.Services
{
    public class GuestbookService : BaseService<Guestbook>, IGuestbookService
    {
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        private readonly IBaseRepository<PasswordLib> _passwordLibRepository;
        private readonly IPasswordLibService _passwordLibService;

        public GuestbookService(IUnitOfWorkManage unitOfWorkManage, IBaseRepository<PasswordLib> passwordLibRepository, IPasswordLibService passwordLibService)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _passwordLibRepository = passwordLibRepository;
            _passwordLibService = passwordLibService;
        }

        public async Task<MessageModel<string>> TestTranInRepository()
        {
            try
            {
                Console.WriteLine($"");
                Console.WriteLine($"事务操作开始");
                using (var uow = _unitOfWorkManage.CreateUnitOfWork())
                {
                    Console.WriteLine($"");
                    Console.WriteLine($"insert a data into the table PasswordLib now.");

                    var insertPassword = await _passwordLibRepository.Add(new PasswordLib
                    {
                        IsDeleted = false,
                        AccountName = "aaa",
                        CreateTime = DateTime.Now
                    });

                    var passwords = await _passwordLibRepository.Query(d => d.IsDeleted == false);
                    Console.WriteLine($"second time : the count of passwords is :{passwords.Count}");

                    Console.WriteLine($"");
                    var guestbooks = await BaseDal.Query();
                    Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

                    int ex = 0;
                    Console.WriteLine($"\nThere's an exception!!");
                    int throwEx = 1 / ex;

                    Console.WriteLine($"insert a data into the table Guestbook now.");
                    var insertGuestbook = await BaseDal.Add(new Guestbook()
                    {
                        UserName = "bbb",
                        BlogId = 1,
                        CreateTime = DateTime.Now,
                        IsShow = true
                    });

                    guestbooks = await BaseDal.Query();
                    Console.WriteLine($"second time : the count of guestbooks is :{guestbooks.Count}");

                    uow.Commit();

                    return new MessageModel<string>
                    {
                        Success = true,
                        Msg = "操作完成"
                    };
                }
            }
            catch (Exception)
            {
                var passwords = await _passwordLibRepository.Query();
                Console.WriteLine($"third time : the count of passwords is :{passwords.Count}");

                var guestbooks = await BaseDal.Query();
                Console.WriteLine($"third time : the count of guestbooks is :{guestbooks.Count}");

                return new MessageModel<string>()
                {
                    Success = false,
                    Msg = "操作异常"
                };
            }
        }

        [UseTran]
        public async Task<bool> TestTranInRepositoryAOP()
        {
            var passwords = await _passwordLibRepository.Query();
            Console.WriteLine($"first time : the count of passwords is :{passwords.Count}");
            Console.WriteLine($"insert a data into the table PasswordLib now.");

            var insertPassword = await _passwordLibRepository.Add(new PasswordLib()
            {
                IsDeleted = false,
                AccountName = "aaa",
                CreateTime = DateTime.Now
            });


            passwords = await _passwordLibRepository.Query(d => d.IsDeleted == false);
            Console.WriteLine($"second time : the count of passwords is :{passwords.Count}");

            //......

            Console.WriteLine($"");
            var guestbooks = await BaseDal.Query();
            Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

            int ex = 0;
            Console.WriteLine($"\nThere's an exception!!");
            int throwEx = 1 / ex;

            Console.WriteLine($"insert a data into the table Guestbook now.");
            var insertGuestbook = await BaseDal.Add(new Guestbook()
            {
                UserName = "bbb",
                BlogId = 1,
                CreateTime = DateTime.Now,
                IsShow = true
            });

            guestbooks = await BaseDal.Query();
            Console.WriteLine($"second time : the count of guestbooks is :{guestbooks.Count}");

            return true;
        }

        /// <summary>
        /// 测试使用同事务
        /// </summary>
        /// <returns></returns>
        [UseTran(Propagation = Propagation.Required)]
        public async Task<bool> TestTranPropagation()
        {
            var guestbooks = await base.Query();
            Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

            var insertGuestbook = await base.Add(new Guestbook
            {
                UserName = "bbb",
                BlogId = 1,
                CreateTime = DateTime.Now,
                IsShow = true
            });

            await _passwordLibService.TestTranPropagation2();

            return true;
        }

        /// <summary>
        /// 测试无事务 Mandatory传播机制报错
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TestTranPropagationNoTran()
        {
            var guestbooks = await base.Query();
            Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

            var insertGuestbook = await base.Add(new Guestbook
            {
                UserName = "bbb",
                BlogId = 1,
                CreateTime = DateTime.Now,
                IsShow = true
            });

            await _passwordLibService.TestTranPropagationNoTranError();

            return true;
        }

        /// <summary>
        /// 测试嵌套事务
        /// </summary>
        /// <returns></returns>
        [UseTran(Propagation = Propagation.Required)]
        public async Task<bool> TestTranPropagationTran()
        {
            var guestbooks = await base.Query();
            Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

            var insertGuestbook = await base.Add(new Guestbook()
            {
                UserName = "bbb",
                BlogId = 1,
                CreateTime = DateTime.Now,
                IsShow = true
            });

            await _passwordLibService.TestTranPropagationTran2();

            return true;
        }
    }
}
