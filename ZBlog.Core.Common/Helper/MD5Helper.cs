﻿using System.Security.Cryptography;
using System.Text;

namespace ZBlog.Core.Common.Helper
{
    public class MD5Helper
    {
        /// <summary>
        /// 16位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt16(string password)
        {
            var md5 = MD5.Create();
            string result = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(password), 4, 8));
            result = result.Replace("-", string.Empty);

            return result;
        }

        /// <summary>
        /// 32位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt32(string password = "")
        {
            try
            {
                string result = string.Empty;

                if (!string.IsNullOrEmpty(password) && !string.IsNullOrWhiteSpace(password))
                {
                    //实例化一个md5对像
                    var md5 = MD5.Create();
                    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
                    byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

                    // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
                    foreach (var item in bytes)
                    {
                        // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                        result = string.Concat(result, item.ToString("X2"));
                    }
                }

                return result;
            }
            catch
            {
                throw new Exception($"错误的 password 字符串:【{password}】");
            }
        }

        /// <summary>
        /// 64位MD5加密
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string MD5Encrypt64(string password)
        {
            // 实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择
            var md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// SHA1加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="format"></param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string SHA1Encrypt(string str, string format = "x2")
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append(item.ToString(format));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sha256加密
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <param name="format"></param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string SHA256Encrypt(string str, string format = "x2")
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA256.Create().ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append(item.ToString(format));
            }

            return sb.ToString();
        }
    }
}
