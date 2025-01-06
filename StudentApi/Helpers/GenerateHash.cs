using System.Security.Cryptography;
using System.Text;

namespace StudentApi.Helpers
{
    public static class GenerateHash
    {
        public static string Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}