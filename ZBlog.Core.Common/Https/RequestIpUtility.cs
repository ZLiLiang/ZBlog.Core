﻿using Microsoft.AspNetCore.Http;
using System.Net;

namespace ZBlog.Core.Common.Https
{
    public static class RequestIpUtility
    {
        public static string GetRequestIp(this HttpContext context)
        {
            string ip = SplitCsv(GetHeaderValueAs<string>(context, "X-Forwarded-For")).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(ip))
                ip = SplitCsv(GetHeaderValueAs<string>(context, "X-Real-IP")).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(ip) && context.Connection?.RemoteIpAddress != null)
                ip = context.Connection.RemoteIpAddress.ToString();

            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeaderValueAs<string>(context, "REMOTE_ADDR");

            return ip;
        }

        public static bool IsLocal(this HttpContext context)
        {
            return GetRequestIp(context) is "127.0.0.1" or "::1" || context.Request?.IsLocal() == true;
        }

        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                else
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;
            else
                return false;
        }

        private static T GetHeaderValueAs<T>(HttpContext context, string headerName)
        {
            if (context.Request?.Headers?.TryGetValue(headerName, out var values) ?? false)
            {
                string rawValues = values.ToString();

                if (!string.IsNullOrWhiteSpace(rawValues))
                    return (T)Convert.ChangeType(rawValues, typeof(T));
            }

            return default;
        }

        private static List<string> SplitCsv(string csvList)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable()
                .Select(s => s.Trim())
                .ToList();
        }
    }
}