using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nacos.V2;
using Nacos.V2.Common;
using Nacos.V2.Naming.Dtos;
using ZBlog.Core.Common.GlobalVars;
using ZBlog.Core.Common.Helper;
using ZBlog.Core.Model;

namespace ZBlog.Core.WebAPI.Controllers
{
    /// <summary>
    /// 服务管理
    /// </summary>
    [Produces("application/json")]
    [Route("api/[Controller]/[action]")]
    [Authorize(Permissions.Name)]
    public class NacosController : BaseApiController
    {
        private readonly INacosNamingService _nacosNamingService;

        public NacosController(INacosNamingService nacosNamingService)
        {
            _nacosNamingService = nacosNamingService;
        }

        /// <summary>
        /// 系统实例是否启动完成
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MessageModel<string> CheckSystemStartFinish()
        {
            //********************* 用当前接口做基本健康检查 确定 基础服务 数据库 缓存都已正常启动*****
            // 然后再进行服务上线
            var data = new MessageModel<string>();
            // ***************  此处请求一下db 跟redis连接等 项目中简介 保证项目已全部启动
            data.Success = true;
            data.Msg = "SUCCESS";

            return data;
        }

        /// <summary>
        /// 获取Nacos 状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> GetStatus()
        {
            var data = new MessageModel<string>();
            var instances = await _nacosNamingService.GetAllInstances(JsonConfigSettings.NacosServiceName);
            if (instances == null || instances.Count == 0)
            {
                data.Status = 406;
                data.Msg = "DOWN";
                data.Success = false;

                return data;
            }

            // 获取当前程序IP
            var currentIp = IpHelper.GetCurrentIp(null);
            bool isUp = false;
            instances.ForEach(it =>
            {
                if (it.Ip == currentIp)
                    isUp = true;
            });
            // var baseUrl = await NacosNamingService.GetServerStatus();
            if (isUp)
            {
                data.Status = 200;
                data.Msg = "UP";
                data.Success = true;

                return data;
            }
            else
            {
                data.Status = 406;
                data.Msg = "DOWN";
                data.Success = false;

                return data;
            }
        }

        /// <summary>
        /// 服务上线
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> Register()
        {
            var data = new MessageModel<string>();
            var instance = new Instance
            {
                ServiceName = JsonConfigSettings.NacosServiceName,
                ClusterName = Constants.DEFAULT_CLUSTER_NAME,
                Ip = IpHelper.GetCurrentIp(null),
                Port = JsonConfigSettings.NacosPort,
                Enabled = true,
                Weight = 100,
                Metadata = JsonConfigSettings.NacosMetadata
            };
            await _nacosNamingService.RegisterInstance(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, instance);
            data.Success = true;
            data.Msg = "SUCCESS";

            return data;
        }

        /// <summary>
        /// 服务下线
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MessageModel<string>> Deregister()
        {
            var data = new MessageModel<string>();
            await _nacosNamingService.DeregisterInstance(JsonConfigSettings.NacosServiceName, Constants.DEFAULT_GROUP, IpHelper.GetCurrentIp(null), JsonConfigSettings.NacosPort);
            data.Success = true;
            data.Msg = "SUCCESS";

            return data;
        }
    }
}
