using System;
using System.Text;
using System.Security.Cryptography;

namespace coinapplib
{
    public class Encrypt
    {
        private static TripleDESCryptoServiceProvider DES = new TripleDESCryptoServiceProvider();
        private static MD5CryptoServiceProvider MD5_ = new MD5CryptoServiceProvider();
        private static readonly MD5 _md5 = MD5.Create();
        
        public static string Enc(string TextToEncypt, string Key)
        {
            try
            {
                DES.Key = MD5Hash(Key);
                DES.Mode = CipherMode.ECB;
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(TextToEncypt);
                return Convert.ToBase64String(DES.CreateEncryptor().TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch
            {
                throw new CryptographicException();
            }
        }

        public static string Dec(string TextToDecrypt, string Key)
        {
            try
            {
                DES.Key = MD5Hash(Key);
                DES.Mode = CipherMode.ECB;
                byte[] Buffer = Convert.FromBase64String(TextToDecrypt);
                return ASCIIEncoding.ASCII.GetString(DES.CreateDecryptor().TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch
            {
                throw new CryptographicException();
            }
        }

        private static byte[] MD5Hash(string value)
        {
            try
            {
                return MD5_.ComputeHash(ASCIIEncoding.ASCII.GetBytes(value));
            }
            catch
            {
                throw new CryptographicException();
            }
        }

        public static string GetMd5Hash(string source)
        {
            try
            {
                var data = _md5.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder sb = new StringBuilder();
                Array.ForEach(data, x => sb.Append(x.ToString("X2")));
                return sb.ToString();
            }
            catch
            {
                throw new CryptographicException();
            }
        }

        public static bool VerifyMd5Hash(string source, string hash)
        {
            try
            {
                var sourceHash = GetMd5Hash(source);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                return comparer.Compare(sourceHash, hash) == 0 ? true : false;
            }
            catch
            {
                throw new CryptographicException();
            }
        }
    }
}
