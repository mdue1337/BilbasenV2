using System.Security.Cryptography;
using System.Text;

namespace MVCtest3d.Other
{
    public class EncryptionHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            // https://www.c-sharpcorner.com/article/compute-sha256-hash-in-c-sharp/
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                StringBuilder builder = new();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
