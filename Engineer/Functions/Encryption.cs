using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    internal class Encryption
    {
        // Aes encryption is used to encrypt the data before sending it to the server
        public static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, string EncodedPassword, SecureString taskKey = null)
        {
            try
            {
                byte[] passwordBytes;
                if (taskKey != null)
                {
                    passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(SecureStringToString(taskKey)));
                    taskKey = null;
                }
                else
                {
                    passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));
                    EncodedPassword = null;
                }
                byte[] encryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 500,HashAlgorithmName.SHA256);
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
        public static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, string EncodedPassword, SecureString taskKey = null)
        {
            try
            {
                byte[] passwordBytes;
                if (taskKey != null)
                {
                    passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(SecureStringToString(taskKey)));
                    taskKey = null;
                }
                else
                {
                    passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(EncodedPassword));
                    EncodedPassword = null;
                }
                byte[] decryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using (MemoryStream ms = new MemoryStream())
                {
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 500, HashAlgorithmName.SHA256);
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
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        private static String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr =  Marshal.SecureStringToBSTR(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(valuePtr);
            }
        }

        //xor byte array with a key return byte array
        public static byte[] Xor(byte[] data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return result;
        }

    }
}
