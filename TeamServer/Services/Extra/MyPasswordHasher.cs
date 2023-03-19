using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TeamServer.Models.Database;

namespace TeamServer.Services.Extra
{
    public class MyPasswordHasher : IPasswordHasher<UserInfo>
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
        
        public string HashPassword(UserInfo user, string password)
        {
            throw new System.NotImplementedException();
        }

        public PasswordVerificationResult VerifyHashedPassword(UserInfo user, string hashedPassword, string providedPassword)
        {
            //verify that the providedPassword hash matches the hashedPassword
            if(providedPassword == hashedPassword)
            {
                return PasswordVerificationResult.Success;
            }
            else
            {
                return PasswordVerificationResult.Failed;
            }
           
        }
    }
}
