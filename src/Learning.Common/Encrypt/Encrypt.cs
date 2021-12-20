using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Learning.Common.Encrypt
{
    public class Encrypt
    {
        /// <summary>
        /// MD5 hash加密
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string MD5(string s)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var t2 = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(s)), 4, 8);
                t2 = t2.Replace("-", "");
                t2 = t2.ToLower();
                return t2;
            }
        }
    }
}
