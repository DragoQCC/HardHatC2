using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    internal class Encryption
    {
        // Aes encryption is used to encrypt the data before sending it to the server
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, string EncodedPassword)
        {
            try
            {

                // make passwordBytes array out of string H@rdH@tC2P@$$w0rd!
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("H@rdH@tC2P@$$w0rd!"));
                //byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));

                byte[] encryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {

                    
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                            cs.Close();
                        }
                        aes.Clear();
                    }
                    encryptedBytes = ms.ToArray();
                }
                return encryptedBytes;
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        // Aes decryption is used to decrypt the data after it has been received from the server
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, string EncodedPassword)
        {
            try
            {
                
                //Console.WriteLine($"decrypting {bytesToBeDecrypted.Length} bytes");
                // make passwordBytes array out of string H@rdH@tC2P@$$w0rd!
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("H@rdH@tC2P@$$w0rd!"));
                //byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));

                byte[] decryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                            cs.Close();
                        }
                        aes.Clear();
                    }
                    decryptedBytes = ms.ToArray();
                }
                return decryptedBytes;
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine(ex.Message);
               // Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
