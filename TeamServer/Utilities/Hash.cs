using System;
using System.Security.Cryptography;
using System.Text;

namespace HardHatCore.TeamServer.Utilities;

public class Hash
{
    private static int keySize = 64;
    private static int iterations = 350000;
    private static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;


    //get the md5 hash of a file
    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(fileName))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return string.Empty;
        }
    }

    //used in register
    public static string HashPassword(string password, out byte[] salt)
    {
        salt = RandomNumberGenerator.GetBytes(keySize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm, keySize);
        return Convert.ToHexString(hash);

    }

    //used in login
    public static string HashPassword(string password, byte[] salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, hashAlgorithm, keySize);
        return Convert.ToHexString(hash);

    }
}