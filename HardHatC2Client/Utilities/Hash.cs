using System.Security.Cryptography;
using System.Text;

namespace HardHatC2Client.Utilities
{
    public class Hash
    {
        private static int keySize = 64;
        private static int iterations = 350000;
        private static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
        
        //used in register
        public static string HashPassword(string password, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = Rfc2898DeriveBytes.Pbkdf2( Encoding.UTF8.GetBytes(password),salt,iterations,hashAlgorithm, keySize);
            return Convert.ToHexString(hash);

        }
        
        //used in login
        public static string HashPassword(string password, byte[] salt)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm, keySize);
            return Convert.ToHexString(hash);

        }

    }
}
