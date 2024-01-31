using Microsoft.Extensions.Options;
using Nacos.V2;
using Ocelot.Provider.Nacos.NacosClient.V2;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using NacosConstants = Nacos.V2.Common.Constants;

namespace Ocelot.Provider.Nacos
{
    public class Nacos : IServiceDiscoveryProvider
    {
        private readonly INacosNamingService _client;
        private readonly string _serviceName;
        private readonly string _groupName;
        private readonly List<string> _clusters;

        public Nacos(string serviceName, INacosNamingService client, IOptions<NacosAspNetOptions> options)
        {
            _serviceName = serviceName;
            _client = client;
            _groupName = string.IsNullOrWhiteSpace(options.Value.GroupName) ?
                NacosConstants.DEFAULT_GROUP : options.Value.GroupName;
            _clusters = (string.IsNullOrWhiteSpace(options.Value.ClusterName) ?
                NacosConstants.DEFAULT_CLUSTER_NAME : options.Value.ClusterName).Split(",").ToList();
        }

        public async Task<List<Service>> GetAsync()
        {
            var services = new List<Service>();
            var instances = await _client.GetAllInstances(_serviceName, _groupName, _clusters);

            if (instances != null && instances.Count != 0)
            {
                foreach (var item in instances)
                {
                    string ip = item.Ip;
                    int port = item.Port;

                    if (item.Metadata.ContainsKey("endpoint"))
                    {
                        string[] ipport = item.Metadata["endpoint"].Split(":");
                        ip = ipport[0];
                        port = int.Parse(ipport[1]);
                    }

                    services.Add(new Service(item.InstanceId, new ServiceHostAndPort(ip, port), "", "", new List<string>()));
                }
            }

            return await Task.FromResult(services);
        }
    }
}
