using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Common.HttpContextUser;
using ZBlog.Core.Common.Https.HttpPolly;
using ZBlog.Core.Common.Option;
using ZBlog.Core.EventBus.Eventbus;
using ZBlog.Core.EventBus.RabbitMQPersistent;
using ZBlog.Core.Extensions.EventHandling;
using ZBlog.Core.Extensions.Redis;
using ZBlog.Core.IServices;
using ZBlog.Core.Model;
using ZBlog.Core.Model.CustomEnums;
using ZBlog.Core.Model.Models;
using ZBlog.Core.Model.ViewModels;
using ZBlog.Core.Services;
using ZBlog.Core.WebAPI.Filter;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// Values控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    //[Authorize(Roles = "Admin,Client")]
    //[Authorize(Policy = "SystemOrAdmin")]
    //[Authorize(PermissionNames.Permission)]
    public class ValuesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IAdvertisementService _advertisementService;
        private readonly IRoleModulePermissionService _roleModulePermissionService;
        private readonly IUser _user;
        private readonly IPasswordLibService _passwordLibService;
        private readonly IBlogArticleService _blogArticleService;
        private readonly IHttpPollyHelper _httpPollyHelper;
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly SeqOptions _seqOptions;

        public ValuesController(
            IMapper mapper,
            IAdvertisementService advertisementService,
            IRoleModulePermissionService roleModulePermissionService,
            IUser user,
            IPasswordLibService passwordLibService,
            IBlogArticleService blogArticleService,
            IHttpPollyHelper httpPollyHelper,
            IRabbitMQPersistentConnection persistentConnection,
            SeqOptions seqOptions)
        {
            _mapper = mapper;
            _advertisementService = advertisementService;
            _roleModulePermissionService = roleModulePermissionService;
            _user = user;
            _passwordLibService = passwordLibService;
            _blogArticleService = blogArticleService;
            _httpPollyHelper = httpPollyHelper;
            _persistentConnection = persistentConnection;
            _seqOptions = seqOptions;
        }

        /// <summary>
        /// 测试Rabbit消息队列发送
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestRabbitMqPublish()
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            _persistentConnection.PublishMessage("Hello, RabbitMQ!", exchangeName: "blogcore", routingKey: "myRoutingKey");

            return Ok();
        }

        /// <summary>
        /// 测试Rabbit消息队列订阅
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult TestRabbitMqSubscribe()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _persistentConnection.StartConsuming("myQueue");
            return Ok();
        }

        private async Task<bool> Dealer(string exchange, string routingKey, byte[] msgBody, IDictionary<string, object> headers)
        {
            await Task.CompletedTask;
            Console.WriteLine("我是消费者，这里消费了一条信息是：" + Encoding.UTF8.GetString(msgBody));
            return true;
        }

        [HttpGet]
        public MessageModel<List<ClaimDto>> MyClaims()
        {
            return new MessageModel<List<ClaimDto>>()
            {
                Success = true,
                Response = _user.GetClaimsIdentity()
                    .Select(it => new ClaimDto
                    {
                        Type = it.Type,
                        Value = it.Value
                    })
                    .ToList()
            };
        }

        /// <summary>
        /// 测试SqlSugar二级缓存
        /// 可设置过期时间
        /// 或通过接口方式更新该数据，也会离开清除缓存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<BlogArticle> TestSqlsugarWithCache()
        {
            return await _blogArticleService.QueryById("1", true);
        }

        /// <summary>
        /// Get方法
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<ResponseEnum>> Get()
        {
            var data = new MessageModel<ResponseEnum>();

            /*
             *  测试 sql 查询
             */
            var queryBySql = await _blogArticleService.QuerySql("SELECT Submitter,Title,Content,CreateTime FROM BlogArticle WHERE Id>5");

            /*
             *  测试按照指定列查询
             */
            var queryByColums = await _blogArticleService.Query(it => new BlogViewModel
            {
                Title = it.Title
            });

            /*
            *  测试按照指定列查询带多条件和排序方法
            */
            Expression<Func<BlogArticle, bool>> registerInfoWhere = a => a.Title == "xxx" && a.Remark == "XXX";
            var queryByColumsByMultiTerms = await _blogArticleService.Query(it => new BlogArticle() { Title = it.Title }, registerInfoWhere, "Id Desc");

            /*
             *  测试 sql 更新
             * 
             * 【SQL参数】：@Id:5
             *  @submitter:laozhang619
             *  @IsDeleted:False
             * 【SQL语句】：UPDATE `BlogArticle`  SET
             *  `submitter`=@submitter,`IsDeleted`=@IsDeleted  WHERE `Id`=@Id
             */
            var updateSql = await _blogArticleService.Update(new
            {
                Submitter = $"laozhang{DateTime.Now.Millisecond}",
                IsDeleted = false,
                Id = 5
            });

            // 测试 AOP 缓存
            var blogArticles = await _blogArticleService.GetBlogs();


            // 测试多表联查
            var roleModulePermissions = await _roleModulePermissionService.QueryMuchTable();


            // 测试多个异步执行时间
            var roleModuleTask = _roleModulePermissionService.Query();
            var listTask = _advertisementService.Query();
            var ad = await roleModuleTask;
            var list = await listTask;


            // 测试service层返回异常
            _advertisementService.ReturnExp();

            return data;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<MessageModel<List<BlogArticle>>> Test_Aop_Cache()
        {
            // 测试 AOP 缓存
            var blogArticles = await _blogArticleService.GetBlogs();

            if (blogArticles.Any())
            {
                return Success(blogArticles);
            }

            return Failed<List<BlogArticle>>();
        }

        /// <summary>
        /// 测试Redis消息队列
        /// </summary>
        /// <param name="_redisBasketRepository"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task RedisMq([FromServices] IRedisBasketRepository _redisBasketRepository)
        {
            var msg = $"这里是一条日志{DateTime.Now}";
            await _redisBasketRepository.ListLeftPushAsync(RedisMqKey.Loging, msg);
        }

        /// <summary>
        /// 测试RabbitMQ事件总线
        /// </summary>
        /// <param name="_eventBus"></param>
        /// <param name="blogId"></param>
        [HttpGet]
        [AllowAnonymous]
        public void EventBusTry([FromServices] IEventBus _eventBus, string blogId = "1")
        {
            var blogDeletedEvent = new BlogQueryIntegrationEvent(blogId);

            _eventBus.Publish(blogDeletedEvent);
        }

        /// <summary>
        /// Get(int id)方法
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [TypeFilter(typeof(UseServiceDIAttribute), Arguments = new object[] { "laozhang" })]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        /// <summary>
        /// 测试参数是必填项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/values/RequiredPara")]
        public string RequiredP([Required] string id)
        {
            return id;
        }

        /// <summary>
        /// 通过 HttpContext 获取用户信息
        /// </summary>
        /// <param name="ClaimType">声明类型，默认 jti </param>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/values/UserInfo")]
        public MessageModel<List<string>> GetUserInfo(string ClaimType = "jti")
        {
            var getUserInfoByToken = _user.GetUserInfoFromToken(ClaimType);
            return new MessageModel<List<string>>()
            {
                Success = _user.IsAuthenticated(),
                Msg = _user.IsAuthenticated() ? _user.Name.ObjToString() : "未登录",
                Response = _user.GetClaimValueByType(ClaimType)
            };
        }

        /// <summary>
        /// to redirect by route template name.
        /// </summary>
        [HttpGet("/api/custom/go-destination")]
        [AllowAnonymous]
        public void Source()
        {
            var url = Url.RouteUrl("Destination_Route");
            Response.Redirect(url);
        }

        /// <summary>
        /// route with template name.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/custom/destination", Name = "Destination_Route")]
        [AllowAnonymous]
        public string Destination()
        {
            return "555";
        }


        /// <summary>
        /// 测试 post 一个对象 + 独立参数
        /// </summary>
        /// <param name="blogArticle">model实体类参数</param>
        /// <param name="id">独立参数</param>
        [HttpPost]
        [AllowAnonymous]
        public object Post([FromBody] BlogArticle blogArticle, int id)
        {
            return Ok(new { success = true, data = blogArticle, id = id });
        }


        /// <summary>
        /// 测试 post 参数
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public object TestPostPara(string name)
        {
            return Ok(new { success = true, name = name });
        }

        /// <summary>
        /// 测试多库连接
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestMutiDBAPI")]
        [AllowAnonymous]
        public async Task<object> TestMutiDBAPI()
        {
            // 从主库中，操作blogs
            var blogs = await _blogArticleService.Query(d => d.Id == 1);
            var addBlog = await _blogArticleService.Add(new BlogArticle() { });

            // 从从库中，操作pwds
            var pwds = await _passwordLibService.Query(d => d.Id > 0);
            var addPwd = await _passwordLibService.Add(new PasswordLib() { });

            return new
            {
                blogs,
                pwds
            };
        }

        /// <summary>
        /// 测试Fulent做参数校验
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> FluentVaTest([FromBody] UserRegisterVo param)
        {
            await Task.CompletedTask;
            return "Okay";
        }

        /// <summary>
        /// Put方法
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        /// <summary>
        /// Delete方法
        /// </summary>
        /// <param name="id"></param>
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        #region Apollo 配置

        /// <summary>
        /// 测试接入Apollo获取配置信息
        /// </summary>
        [HttpGet("/apollo")]
        [AllowAnonymous]
        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllConfigByAppllo(
            [FromServices] IConfiguration configuration)
        {
            return await Task.FromResult(configuration.AsEnumerable());
        }

        /// <summary>
        /// 通过此处的key格式为 xx:xx:x
        /// </summary>
        [HttpGet("/apollo/{key}")]
        [AllowAnonymous]
        public async Task<string> GetConfigByAppllo(string key)
        {
            return await Task.FromResult(AppSettings.App(key));
        }

        #endregion

        #region HttpPolly

        [HttpPost]
        [AllowAnonymous]
        public async Task<string> HttpPollyPost()
        {
            var response = await _httpPollyHelper.PostAsync(HttpEnum.LocalHost, "/api/ElasticDemo/EsSearchTest",
                "{\"from\": 0,\"size\": 10,\"word\": \"非那雄安\"}");

            return response;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<string> HttpPollyGet()
        {
            return await _httpPollyHelper.GetAsync(HttpEnum.LocalHost,
                "/api/ElasticDemo/GetDetailInfo?esid=3130&esindex=chinacodex");
        }

        #endregion

        [HttpPost]
        [AllowAnonymous]
        public string TestEnum(EnumDemoDto dto) => dto.Type.ToString();

        [HttpGet]
        [AllowAnonymous]
        public string TestOption()
        {
            return _seqOptions.ToJson();
        }
    }

    public class ClaimDto
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
