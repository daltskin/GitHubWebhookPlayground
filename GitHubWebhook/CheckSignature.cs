using System;
using System.Security.Cryptography;
using System.Text;

namespace GitHubWebhook
{
    public static class CheckSignature
    {
        public static bool Validate (string signature, string body, string secret)
        {
            string expectedSignature = "sha1=" + HMACSHA256(secret, body);
            return expectedSignature.Equals(signature);
        }

        private static string HMACSHA256(string key, string data)
        {
            string hash;
            ASCIIEncoding encoder = new ASCIIEncoding();
            Byte[] code = encoder.GetBytes(key);
            using (HMACSHA1 hmac = new HMACSHA1(code))
            {
                Byte[] hmBytes = hmac.ComputeHash(encoder.GetBytes(data));
                hash = ToHexString(hmBytes);
            }
            return hash;
        }

        private static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
