﻿using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ZBlog.Core.Common.Helper
{
    public class IpHelper
    {
        /// <summary>
        /// 获取当前IP地址
        /// </summary>
        /// <param name="preferredNetworks"></param>
        /// <returns></returns>
        public static string GetCurrentIp(string preferredNetworks)
        {
            var instanceIp = "127.0.0.1";

            try
            {
                // 获取可用网卡 
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(network => network.OperationalStatus == OperationalStatus.Up);

                // 获取所有可用网卡IP信息
                var ipCollection = networkInterfaces.Select(network => network.GetIPProperties())
                    .SelectMany(ip => ip.UnicastAddresses);

                foreach (var ipadd in ipCollection)
                {
                    if (!IPAddress.IsLoopback(ipadd.Address) && ipadd.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (string.IsNullOrEmpty(preferredNetworks))
                        {
                            instanceIp = ipadd.Address.ToString();
                            break;
                        }

                        if (!ipadd.Address.ToString().StartsWith(preferredNetworks))
                            continue;
                        instanceIp = ipadd.Address.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return instanceIp;
        }
    }
}
