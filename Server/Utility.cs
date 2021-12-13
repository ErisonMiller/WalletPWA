using System;
using System.Security.Cryptography;
using System.Text;

namespace WalletPWA.Server
{
    public class Utility
    {
        public static string Encrypt(string password)
        {
            var provider = MD5.Create();
            string salt = "¨^_Miller~Ç@llT_^¨";
            byte[] bytes = provider.ComputeHash(Encoding.UTF32.GetBytes(salt + password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }
}
